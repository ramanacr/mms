using MeetingScheduler.Api.Dtos;
using MeetingScheduler.Api.Models;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using MeetingScheduler.Api.Options;

namespace MeetingScheduler.Api.Services;

public sealed class GraphCalendarService : IGraphCalendarService
{
    private readonly GraphServiceClient? _graphClient;
    private readonly ILogger<GraphCalendarService> _logger;
    private readonly GraphOptions _options;

    public GraphCalendarService(GraphServiceClient? graphClient, IOptions<GraphOptions> options, ILogger<GraphCalendarService> logger)
    {
        _graphClient = graphClient;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<string?> CreateEventAsync(MeetingRoom room, CreateBookingRequest request, IReadOnlyList<(DateTime StartAt, DateTime EndAt)> instances, CancellationToken cancellationToken = default)
    {
        if (_graphClient is null)
        {
            _logger.LogInformation("Graph client is not configured; skipping Outlook event creation.");
            return null;
        }

        var first = instances.First();
        var evt = new Event
        {
            Subject = request.Subject,
            Start = new DateTimeTimeZone { DateTime = first.StartAt.ToString("yyyy-MM-ddTHH:mm:ss"), TimeZone = request.TimeZone },
            End = new DateTimeTimeZone { DateTime = first.EndAt.ToString("yyyy-MM-ddTHH:mm:ss"), TimeZone = request.TimeZone },
            Location = new Location { DisplayName = room.Name, LocationEmailAddress = room.ExchangeEmail },
            Attendees = CreateAttendees(room, request.Attendees, request.OptionalAttendees ?? []),
            Body = CreateBody(request.Body)
        };

        if (request.Recurrence is { Type: not null } recurrence
            && !string.Equals(recurrence.Type, "None", StringComparison.OrdinalIgnoreCase))
        {
            evt.Recurrence = CreatePatternedRecurrence(request, recurrence);
        }

        try
        {
            var created = await _graphClient.Me.Events.PostAsync(evt, cancellationToken: cancellationToken);
            return created?.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Microsoft Graph event with scopes {Scopes}", string.Join(",", _options.Scopes));
            throw;
        }
    }

    public async Task DeleteEventAsync(string graphEventId, CancellationToken cancellationToken = default)
    {
        if (_graphClient is null || string.IsNullOrWhiteSpace(graphEventId))
        {
            return;
        }

        await _graphClient.Me.Events[graphEventId].DeleteAsync(cancellationToken: cancellationToken);
    }

    public static List<Attendee> CreateAttendees(MeetingRoom room, IEnumerable<string> requiredEmails, IEnumerable<string> optionalEmails)
    {
        var roomEmail = string.IsNullOrWhiteSpace(room.ExchangeEmail)
            ? null
            : room.ExchangeEmail.Trim();

        var required = NormalizeEmails(requiredEmails, roomEmail).ToList();
        var optional = NormalizeEmails(optionalEmails, roomEmail)
            .Where(a => !required.Contains(a, StringComparer.OrdinalIgnoreCase))
            .ToList();

        var attendees = required
            .Select(a => CreateAttendee(a, AttendeeType.Required))
            .Concat(optional.Select(a => CreateAttendee(a, AttendeeType.Optional)))
            .ToList();

        if (roomEmail is not null)
        {
            attendees.Add(CreateAttendee(roomEmail, AttendeeType.Resource, room.Name));
        }

        return attendees;
    }

    public static ItemBody? CreateBody(string? body)
    {
        return string.IsNullOrWhiteSpace(body)
            ? null
            : new ItemBody
            {
                ContentType = BodyType.Html,
                Content = body.Trim()
            };
    }

    private static IEnumerable<string> NormalizeEmails(IEnumerable<string> emails, string? roomEmail)
    {
        return emails
            .Where(a => !string.IsNullOrWhiteSpace(a))
            .Select(a => a.Trim())
            .Where(a => roomEmail is null || !string.Equals(a, roomEmail, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase);
    }

    private static Attendee CreateAttendee(string address, AttendeeType type, string? name = null) =>
        new()
        {
            Type = type,
            EmailAddress = new EmailAddress { Address = address, Name = name ?? address }
        };

    private static PatternedRecurrence CreatePatternedRecurrence(CreateBookingRequest request, RecurrenceRequest recurrence)
    {
        var type = recurrence.Type.ToLowerInvariant() switch
        {
            "daily" => RecurrencePatternType.Daily,
            "weekly" => RecurrencePatternType.Weekly,
            "monthly" => RecurrencePatternType.AbsoluteMonthly,
            _ => RecurrencePatternType.Daily
        };

        var pattern = new RecurrencePattern
        {
            Type = type,
            Interval = recurrence.Interval < 1 ? 1 : recurrence.Interval
        };

        if (type == RecurrencePatternType.Weekly)
        {
            pattern.DaysOfWeek = [ToGraphDay(request.StartAt.DayOfWeek)];
        }

        if (type == RecurrencePatternType.AbsoluteMonthly)
        {
            pattern.DayOfMonth = request.StartAt.Day;
        }

        return new PatternedRecurrence
        {
            Pattern = pattern,
            Range = new RecurrenceRange
            {
                Type = RecurrenceRangeType.EndDate,
                StartDate = DateOnly.FromDateTime(request.StartAt),
                EndDate = DateOnly.FromDateTime(recurrence.Until ?? request.StartAt.AddMonths(6))
            }
        };
    }

    private static DayOfWeekObject ToGraphDay(DayOfWeek day) =>
        day switch
        {
            DayOfWeek.Monday => DayOfWeekObject.Monday,
            DayOfWeek.Tuesday => DayOfWeekObject.Tuesday,
            DayOfWeek.Wednesday => DayOfWeekObject.Wednesday,
            DayOfWeek.Thursday => DayOfWeekObject.Thursday,
            DayOfWeek.Friday => DayOfWeekObject.Friday,
            DayOfWeek.Saturday => DayOfWeekObject.Saturday,
            _ => DayOfWeekObject.Sunday
        };
}
