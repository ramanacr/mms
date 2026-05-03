using MeetingScheduler.Api.Dtos;

namespace MeetingScheduler.Api.Services;

public interface IRoomService
{
    Task<IReadOnlyList<RoomDto>> GetRoomsAsync(CancellationToken cancellationToken = default);
    Task<RoomDto> CreateRoomAsync(UpsertRoomRequest request, CancellationToken cancellationToken = default);
    Task<RoomDto> UpdateRoomAsync(Guid id, UpsertRoomRequest request, CancellationToken cancellationToken = default);
    Task DeleteRoomAsync(Guid id, CancellationToken cancellationToken = default);
}
