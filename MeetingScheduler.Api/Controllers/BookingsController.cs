using MeetingScheduler.Api.Dtos;
using MeetingScheduler.Api.Infrastructure;
using MeetingScheduler.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeetingScheduler.Api.Controllers;

public sealed class BookingsController : TenantControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService, ITenantProvider tenantProvider) : base(tenantProvider)
    {
        _bookingService = bookingService;
    }

    [HttpGet]
    [Authorize(Policy = "OrgUser")]
    public async Task<ActionResult<IReadOnlyList<BookingInstanceDto>>> Get([FromQuery] DateTime start, [FromQuery] DateTime end, CancellationToken cancellationToken)
    {
        if (start == default || end == default)
        {
            end = DateTime.UtcNow.Date.AddDays(30);
            start = DateTime.UtcNow.Date.AddDays(-1);
        }

        return Ok(await _bookingService.GetBookingsAsync(start, end, cancellationToken));
    }

    [HttpPost]
    [Authorize(Policy = "OrgUser")]
    public async Task<ActionResult<CreateBookingResponse>> Create(CreateBookingRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _bookingService.CreateBookingAsync(request, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "OrgUser")]
    public async Task<ActionResult<BookingInstanceDto>> Update(Guid id, UpdateBookingRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _bookingService.UpdateBookingAsync(id, request, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "OrgUser")]
    public async Task<IActionResult> Delete(Guid id, [FromQuery] bool deleteSeries, CancellationToken cancellationToken)
    {
        await _bookingService.DeleteBookingAsync(id, deleteSeries, cancellationToken);
        return NoContent();
    }
}
