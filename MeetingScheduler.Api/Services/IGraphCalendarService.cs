using MeetingScheduler.Api.Dtos;
using MeetingScheduler.Api.Models;

namespace MeetingScheduler.Api.Services;

public interface IGraphCalendarService
{
    Task<string?> CreateEventAsync(MeetingRoom room, CreateBookingRequest request, IReadOnlyList<(DateTime StartAt, DateTime EndAt)> instances, CancellationToken cancellationToken = default);
    Task DeleteEventAsync(string graphEventId, CancellationToken cancellationToken = default);
}
