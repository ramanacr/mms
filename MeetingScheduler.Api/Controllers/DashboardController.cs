using MeetingScheduler.Api.Data;
using MeetingScheduler.Api.Dtos;
using MeetingScheduler.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MeetingScheduler.Api.Controllers;

public sealed class DashboardController : TenantControllerBase
{
    private readonly AppDbContext _dbContext;

    public DashboardController(AppDbContext dbContext, ITenantProvider tenantProvider) : base(tenantProvider)
    {
        _dbContext = dbContext;
    }

    [HttpGet("stats")]
    public async Task<ActionResult<DashboardStatsDto>> Stats(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var rooms = await _dbContext.MeetingRooms.CountAsync(cancellationToken);
        var bookingsToday = await _dbContext.BookingInstances
            .CountAsync(b => b.StartAt < tomorrow && b.EndAt > today, cancellationToken);
        var consentGranted = await _dbContext.Tenants
            .IgnoreQueryFilters()
            .AnyAsync(t => t.MicrosoftTenantId == TenantProvider.MicrosoftTenantId && t.IsActive, cancellationToken);

        return Ok(new DashboardStatsDto(rooms, bookingsToday, consentGranted, consentGranted));
    }
}
