using MeetingScheduler.Api.Data;
using MeetingScheduler.Api.Dtos;
using MeetingScheduler.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MeetingScheduler.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TenantsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public TenantsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost("admin-consent")]
    [AllowAnonymous]
    public async Task<ActionResult> AdminConsent(AdminConsentRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.MicrosoftTenantId))
        {
            return BadRequest(new { error = "Microsoft tenant id is required." });
        }

        var tenant = await _dbContext.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.MicrosoftTenantId == request.MicrosoftTenantId, cancellationToken);

        if (tenant is null)
        {
            tenant = new Tenant
            {
                MicrosoftTenantId = request.MicrosoftTenantId.Trim(),
                OrganizationName = string.IsNullOrWhiteSpace(request.OrganizationName) ? request.MicrosoftTenantId.Trim() : request.OrganizationName.Trim(),
                CustomDomain = request.CustomDomain?.Trim()
            };
            _dbContext.Tenants.Add(tenant);
        }
        else
        {
            tenant.OrganizationName = string.IsNullOrWhiteSpace(request.OrganizationName) ? tenant.OrganizationName : request.OrganizationName.Trim();
            tenant.CustomDomain = request.CustomDomain?.Trim();
            tenant.IsActive = true;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Ok(new { tenant.Id, tenant.MicrosoftTenantId, tenant.OrganizationName });
    }
}
