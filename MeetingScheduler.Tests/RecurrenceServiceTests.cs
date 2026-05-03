using MeetingScheduler.Api.Dtos;
using MeetingScheduler.Api.Services;

namespace MeetingScheduler.Tests;

public sealed class RecurrenceServiceTests
{
    private readonly RecurrenceService _service = new();

    [Fact]
    public void Expands_single_booking_once()
    {
        var result = _service.Expand(new DateTime(2026, 5, 1, 9, 0, 0), new DateTime(2026, 5, 1, 10, 0, 0), null);

        Assert.Single(result);
        Assert.Equal(new DateTime(2026, 5, 1, 9, 0, 0, DateTimeKind.Utc), result[0].StartAt);
    }

    [Fact]
    public void Expands_weekly_recurrence_until_date()
    {
        var result = _service.Expand(
            new DateTime(2026, 5, 4, 9, 0, 0),
            new DateTime(2026, 5, 4, 10, 0, 0),
            new RecurrenceRequest("Weekly", 1, new DateTime(2026, 5, 18, 9, 0, 0)));

        Assert.Equal(3, result.Count);
        Assert.Equal(new DateTime(2026, 5, 11, 9, 0, 0, DateTimeKind.Utc), result[1].StartAt);
    }

    [Fact]
    public void Expands_monthly_recurrence()
    {
        var result = _service.Expand(
            new DateTime(2026, 1, 15, 9, 0, 0),
            new DateTime(2026, 1, 15, 10, 0, 0),
            new RecurrenceRequest("Monthly", 1, new DateTime(2026, 3, 15, 9, 0, 0)));

        Assert.Equal(3, result.Count);
        Assert.Equal(new DateTime(2026, 2, 15, 9, 0, 0, DateTimeKind.Utc), result[1].StartAt);
    }
}
