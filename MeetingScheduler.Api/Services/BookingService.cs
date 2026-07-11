using MeetingScheduler.Api.Dtos;
using MeetingScheduler.Api.Infrastructure;
using MeetingScheduler.Api.Models;
using MeetingScheduler.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MeetingScheduler.Api.Services;

public sealed class BookingService : IBookingService
{
    private readonly IRepository<MeetingRoom> _rooms;
    private readonly IRepository<BookingSeries> _series;
    private readonly IRepository<BookingInstance> _instances;
    private readonly IRecurrenceService _recurrenceService;
    private readonly IGraphCalendarService _graphCalendarService;
    private readonly ITenantProvider _tenantProvider;

    public BookingService(
        IRepository<MeetingRoom> rooms,
        IRepository<BookingSeries> series,
        IRepository<BookingInstance> instances,
        IRecurrenceService recurrenceService,
        IGraphCalendarService graphCalendarService,
        ITenantProvider tenantProvider)
    {
        _rooms = rooms;
        _series = series;
        _instances = instances;
        _recurrenceService = recurrenceService;
        _graphCalendarService = graphCalendarService;
        _tenantProvider = tenantProvider;
    }

    public async Task<IReadOnlyList<BookingInstanceDto>> GetBookingsAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default)
    {
        var startUtc = ToUtc(start);
        var endUtc = ToUtc(end);

        return await _instances.Query()
            .Include(b => b.Room)
            .Where(b => b.StartAt < endUtc && b.EndAt > startUtc)
            .OrderBy(b => b.StartAt)
            .Select(b => ToDto(b))
            .ToListAsync(cancellationToken);
    }

    public async Task<CreateBookingResponse> CreateBookingAsync(CreateBookingRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);
        var room = await _rooms.Query().FirstOrDefaultAsync(r => r.Id == request.RoomId && r.IsActive, cancellationToken)
            ?? throw new ArgumentException("Room was not found or is inactive.");

        var expanded = _recurrenceService.Expand(request.StartAt, request.EndAt, request.Recurrence);
        var hasConflict = await HasConflictAsync(request.RoomId, expanded, cancellationToken);
        if (hasConflict)
        {
            throw new ConflictException("The selected room is already booked for one or more requested times.");
        }

        var graphId = await _graphCalendarService.CreateEventAsync(room, request, expanded, cancellationToken);
        var recurrenceType = request.Recurrence?.Type ?? "None";
        var interval = Math.Max(1, request.Recurrence?.Interval ?? 1);
        var organizer = string.IsNullOrWhiteSpace(_tenantProvider.UserEmail) ? "unknown@local" : _tenantProvider.UserEmail;
        var attendeeCsv = string.Join(';', request.Attendees
            .Concat(request.OptionalAttendees ?? [])
            .Where(a => !string.IsNullOrWhiteSpace(a))
            .Select(a => a.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase));

        var series = new BookingSeries
        {
            RoomId = room.Id,
            Subject = request.Subject.Trim(),
            OrganizerEmail = organizer,
            Attendees = attendeeCsv,
            TimeZone = string.IsNullOrWhiteSpace(request.TimeZone) ? "UTC" : request.TimeZone.Trim(),
            GraphSeriesId = graphId,
            RecurrenceType = recurrenceType,
            Interval = interval,
            StartDate = expanded.Min(i => i.StartAt),
            EndDate = expanded.Max(i => i.EndAt)
        };

        foreach (var occurrence in expanded)
        {
            series.Instances.Add(new BookingInstance
            {
                RoomId = room.Id,
                Subject = series.Subject,
                OrganizerEmail = organizer,
                Attendees = attendeeCsv,
                StartAt = occurrence.StartAt,
                EndAt = occurrence.EndAt,
                GraphInstanceId = expanded.Count == 1 ? graphId : null
            });
        }

        await _series.AddAsync(series, cancellationToken);
        await _series.SaveChangesAsync(cancellationToken);

        var dtos = series.Instances
            .OrderBy(i => i.StartAt)
            .Select(i => new BookingInstanceDto(i.Id, series.Id, room.Id, room.Name, i.Subject, i.OrganizerEmail, SplitAttendees(i.Attendees), i.StartAt, i.EndAt, expanded.Count > 1))
            .ToList();

        return new CreateBookingResponse(series.Id, dtos);
    }

    public async Task DeleteBookingAsync(Guid id, bool deleteSeries = false, CancellationToken cancellationToken = default)
    {
        var instance = await _instances.Query()
            .Include(i => i.Series)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Booking was not found.");

        if (deleteSeries && instance.Series is not null)
        {
            if (!string.IsNullOrWhiteSpace(instance.Series.GraphSeriesId))
            {
                await _graphCalendarService.DeleteEventAsync(instance.Series.GraphSeriesId, cancellationToken);
            }

            _series.Remove(instance.Series);
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(instance.GraphInstanceId))
            {
                await _graphCalendarService.DeleteEventAsync(instance.GraphInstanceId, cancellationToken);
            }

            _instances.Remove(instance);
        }

        await _instances.SaveChangesAsync(cancellationToken);
    }

    private async Task<bool> HasConflictAsync(Guid roomId, IReadOnlyList<(DateTime StartAt, DateTime EndAt)> expanded, CancellationToken cancellationToken)
    {
        foreach (var occurrence in expanded)
        {
            var conflict = await _instances.Query()
                .AnyAsync(b => b.RoomId == roomId && b.StartAt < occurrence.EndAt && b.EndAt > occurrence.StartAt, cancellationToken);

            if (conflict)
            {
                return true;
            }
        }

        return false;
    }

    private static void Validate(CreateBookingRequest request)
    {
        if (request.RoomId == Guid.Empty) throw new ArgumentException("Room is required.");
        if (string.IsNullOrWhiteSpace(request.Subject)) throw new ArgumentException("Subject is required.");
        if (request.EndAt <= request.StartAt) throw new ArgumentException("End time must be after start time.");
    }

    private static BookingInstanceDto ToDto(BookingInstance instance) =>
        new(instance.Id, instance.SeriesId, instance.RoomId, instance.Room.Name, instance.Subject, instance.OrganizerEmail, SplitAttendees(instance.Attendees), instance.StartAt, instance.EndAt, instance.SeriesId is not null);

    private static string[] SplitAttendees(string? attendees) =>
        string.IsNullOrWhiteSpace(attendees)
            ? []
            : attendees.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static DateTime ToUtc(DateTime value) =>
        value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
}
