namespace MeetingScheduler.Api.Dtos;

public sealed record RecurrenceRequest(string Type, int Interval, DateTime? Until);

public sealed record CreateBookingRequest(
    Guid RoomId,
    string Subject,
    string[] Attendees,
    DateTime StartAt,
    DateTime EndAt,
    string TimeZone,
    RecurrenceRequest? Recurrence);

public sealed record BookingInstanceDto(
    Guid Id,
    Guid? SeriesId,
    Guid RoomId,
    string RoomName,
    string Subject,
    string OrganizerEmail,
    string[] Attendees,
    DateTime StartAt,
    DateTime EndAt,
    bool IsRecurring);

public sealed record CreateBookingResponse(Guid SeriesId, IReadOnlyList<BookingInstanceDto> Instances);
