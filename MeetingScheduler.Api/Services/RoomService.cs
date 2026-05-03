using MeetingScheduler.Api.Dtos;
using MeetingScheduler.Api.Models;
using MeetingScheduler.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MeetingScheduler.Api.Services;

public sealed class RoomService : IRoomService
{
    private readonly IRepository<MeetingRoom> _rooms;

    public RoomService(IRepository<MeetingRoom> rooms)
    {
        _rooms = rooms;
    }

    public async Task<IReadOnlyList<RoomDto>> GetRoomsAsync(CancellationToken cancellationToken = default)
    {
        return await _rooms.Query()
            .OrderBy(r => r.Floor)
            .ThenBy(r => r.Name)
            .Select(r => ToDto(r))
            .ToListAsync(cancellationToken);
    }

    public async Task<RoomDto> CreateRoomAsync(UpsertRoomRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);
        var room = new MeetingRoom
        {
            Name = request.Name.Trim(),
            Floor = request.Floor.Trim(),
            Location = request.Location.Trim(),
            Capacity = request.Capacity,
            Amenities = request.Amenities?.Trim(),
            ExchangeEmail = request.ExchangeEmail?.Trim(),
            IsActive = request.IsActive
        };

        await _rooms.AddAsync(room, cancellationToken);
        await _rooms.SaveChangesAsync(cancellationToken);
        return ToDto(room);
    }

    public async Task<RoomDto> UpdateRoomAsync(Guid id, UpsertRoomRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);
        var room = await _rooms.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("Room was not found.");

        room.Name = request.Name.Trim();
        room.Floor = request.Floor.Trim();
        room.Location = request.Location.Trim();
        room.Capacity = request.Capacity;
        room.Amenities = request.Amenities?.Trim();
        room.ExchangeEmail = request.ExchangeEmail?.Trim();
        room.IsActive = request.IsActive;

        _rooms.Update(room);
        await _rooms.SaveChangesAsync(cancellationToken);
        return ToDto(room);
    }

    public async Task DeleteRoomAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var room = await _rooms.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("Room was not found.");

        _rooms.Remove(room);
        await _rooms.SaveChangesAsync(cancellationToken);
    }

    private static void Validate(UpsertRoomRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name)) throw new ArgumentException("Room name is required.");
        if (string.IsNullOrWhiteSpace(request.Floor)) throw new ArgumentException("Floor is required.");
        if (string.IsNullOrWhiteSpace(request.Location)) throw new ArgumentException("Location is required.");
        if (request.Capacity < 1) throw new ArgumentException("Capacity must be at least 1.");
    }

    private static RoomDto ToDto(MeetingRoom room) =>
        new(room.Id, room.Name, room.Floor, room.Location, room.Capacity, room.Amenities, room.ExchangeEmail, room.IsActive);
}
