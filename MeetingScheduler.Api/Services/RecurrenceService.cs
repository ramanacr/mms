using MeetingScheduler.Api.Dtos;

namespace MeetingScheduler.Api.Services;

public sealed class RecurrenceService : IRecurrenceService
{
    private const int MaxInstances = 370;
    private static readonly TimeSpan DefaultProjection = TimeSpan.FromDays(180);

    public IReadOnlyList<(DateTime StartAt, DateTime EndAt)> Expand(DateTime startAt, DateTime endAt, RecurrenceRequest? recurrence)
    {
        if (endAt <= startAt)
        {
            throw new ArgumentException("End time must be after start time.");
        }

        var normalizedType = recurrence?.Type?.Trim().ToLowerInvariant() ?? "none";
        if (normalizedType is "" or "none")
        {
            return [(ToUtc(startAt), ToUtc(endAt))];
        }

        var interval = Math.Max(1, recurrence?.Interval ?? 1);
        var until = ToUtc(recurrence?.Until ?? startAt.Add(DefaultProjection));
        var duration = endAt - startAt;
        var cursor = ToUtc(startAt);
        var instances = new List<(DateTime StartAt, DateTime EndAt)>();

        while (cursor <= until && instances.Count < MaxInstances)
        {
            instances.Add((cursor, cursor + duration));
            cursor = normalizedType switch
            {
                "daily" => cursor.AddDays(interval),
                "weekly" => cursor.AddDays(7 * interval),
                "monthly" => cursor.AddMonths(interval),
                _ => throw new ArgumentException("Recurrence type must be None, Daily, Weekly, or Monthly.")
            };
        }

        return instances;
    }

    private static DateTime ToUtc(DateTime value) =>
        value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
}
