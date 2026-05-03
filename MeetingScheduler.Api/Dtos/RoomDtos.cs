namespace MeetingScheduler.Api.Dtos;

public sealed record RoomDto(
    Guid Id,
    string Name,
    string Floor,
    string Location,
    int Capacity,
    string? Amenities,
    string? ExchangeEmail,
    bool IsActive);

public sealed record UpsertRoomRequest(
    string Name,
    string Floor,
    string Location,
    int Capacity,
    string? Amenities,
    string? ExchangeEmail,
    bool IsActive);
