using MeetingScheduler.Api.Dtos;
using MeetingScheduler.Api.Models;
using MeetingScheduler.Api.Services;

namespace MeetingScheduler.Tests.Fakes;

public sealed record CapturedGraphEvent(MeetingRoom Room, CreateBookingRequest Request, IReadOnlyList<(DateTime StartAt, DateTime EndAt)> Instances);

public sealed class FakeGraphCalendarService : IGraphCalendarService
{
    public List<CapturedGraphEvent> CreatedEvents { get; } = [];
    public List<string> DeletedIds { get; } = [];

    public Task<string?> CreateEventAsync(MeetingRoom room, CreateBookingRequest request, IReadOnlyList<(DateTime StartAt, DateTime EndAt)> instances, CancellationToken cancellationToken = default)
    {
        CreatedEvents.Add(new CapturedGraphEvent(room, request, instances));
        return Task.FromResult<string?>($"graph-{Guid.NewGuid():N}");
    }

    public Task DeleteEventAsync(string graphEventId, CancellationToken cancellationToken = default)
    {
        DeletedIds.Add(graphEventId);
        return Task.CompletedTask;
    }
}
