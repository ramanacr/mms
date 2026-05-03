using MeetingScheduler.Api.Dtos;
using MeetingScheduler.Api.Infrastructure;
using MeetingScheduler.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeetingScheduler.Api.Controllers;

public sealed class RoomsController : TenantControllerBase
{
    private readonly IRoomService _roomService;

    public RoomsController(IRoomService roomService, ITenantProvider tenantProvider) : base(tenantProvider)
    {
        _roomService = roomService;
    }

    [HttpGet]
    [Authorize(Policy = "OrgUser")]
    public async Task<ActionResult<IReadOnlyList<RoomDto>>> Get(CancellationToken cancellationToken)
    {
        return Ok(await _roomService.GetRoomsAsync(cancellationToken));
    }

    [HttpPost]
    [Authorize(Policy = "OrgAdmin")]
    public async Task<ActionResult<RoomDto>> Create(UpsertRoomRequest request, CancellationToken cancellationToken)
    {
        var created = await _roomService.CreateRoomAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "OrgAdmin")]
    public async Task<ActionResult<RoomDto>> Update(Guid id, UpsertRoomRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _roomService.UpdateRoomAsync(id, request, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "OrgAdmin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _roomService.DeleteRoomAsync(id, cancellationToken);
        return NoContent();
    }
}
