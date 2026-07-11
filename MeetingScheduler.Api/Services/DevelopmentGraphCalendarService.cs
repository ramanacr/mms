using MeetingScheduler.Api.Dtos;
using MeetingScheduler.Api.Models;

namespace MeetingScheduler.Api.Services;

public sealed class DevelopmentGraphCalendarService : IGraphCalendarService
{
    public Task<string?> CreateEventAsync(
        MeetingRoom room,
        CreateBookingRequest request,
        IReadOnlyList<(DateTime StartAt, DateTime EndAt)> instances,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<string?>(null);
    }

    public Task DeleteEventAsync(string graphEventId, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
