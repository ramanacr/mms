using MeetingScheduler.Api.Dtos;

namespace MeetingScheduler.Api.Services;

public interface IRecurrenceService
{
    IReadOnlyList<(DateTime StartAt, DateTime EndAt)> Expand(DateTime startAt, DateTime endAt, RecurrenceRequest? recurrence);
}
