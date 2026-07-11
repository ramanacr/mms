using MeetingScheduler.Api.Data;
using MeetingScheduler.Api.Dtos;
using MeetingScheduler.Api.Infrastructure;
using MeetingScheduler.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace MeetingScheduler.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TenantsController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;
    private readonly IConfiguration _configuration;

    public TenantsController(AppDbContext dbContext, ITenantProvider tenantProvider, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
        _configuration = configuration;
    }

    [HttpPost("admin-consent/start")]
    [Authorize]
    public async Task<ActionResult<AdminConsentStartResponse>> StartAdminConsent(CancellationToken cancellationToken)
    {
        if (!_tenantProvider.HasTenant)
        {
            return BadRequest(new { error = "Tenant claim is missing." });
        }

        var state = CreateState();
        var pending = new PendingAdminConsent
        {
            State = state,
            ExpectedMicrosoftTenantId = _tenantProvider.MicrosoftTenantId,
            RequestedByEmail = string.IsNullOrWhiteSpace(_tenantProvider.UserEmail) ? "unknown" : _tenantProvider.UserEmail,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15)
        };

        _dbContext.PendingAdminConsents.Add(pending);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var clientId = _configuration["AzureAd:ClientId"];
        if (string.IsNullOrWhiteSpace(clientId))
        {
            return BadRequest(new { error = "Azure AD client id is not configured." });
        }

        var redirectUri = ResolveAdminConsentRedirectUri();
        var consentUrl = "https://login.microsoftonline.com/common/adminconsent"
            + $"?client_id={Uri.EscapeDataString(clientId)}"
            + $"&redirect_uri={Uri.EscapeDataString(redirectUri)}"
            + $"&state={Uri.EscapeDataString(state)}";

        return Ok(new AdminConsentStartResponse(consentUrl));
    }

    [HttpPost("admin-consent")]
    [AllowAnonymous]
    public async Task<ActionResult> CompleteAdminConsent(AdminConsentCompleteRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.State))
        {
            return BadRequest(new { error = "Admin consent state is required." });
        }

        if (!request.AdminConsentGranted)
        {
            return BadRequest(new { error = "Admin consent was not granted." });
        }

        if (string.IsNullOrWhiteSpace(request.MicrosoftTenantId))
        {
            return BadRequest(new { error = "Microsoft tenant id is required." });
        }

        var returnedTenantId = request.MicrosoftTenantId.Trim();
        var state = request.State.Trim();
        var pending = await _dbContext.PendingAdminConsents
            .FirstOrDefaultAsync(c => c.State == state, cancellationToken);

        if (pending is null)
        {
            return BadRequest(new { error = "Admin consent state is invalid." });
        }

        if (pending.UsedAt is not null)
        {
            return BadRequest(new { error = "Admin consent state was already used." });
        }

        if (pending.ExpiresAt <= DateTime.UtcNow)
        {
            return BadRequest(new { error = "Admin consent state has expired." });
        }

        if (!string.Equals(pending.ExpectedMicrosoftTenantId, returnedTenantId, StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { error = "Admin consent tenant does not match the requested tenant." });
        }

        var tenant = await _dbContext.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.MicrosoftTenantId == returnedTenantId, cancellationToken);

        if (tenant is null)
        {
            tenant = new Tenant
            {
                Id = Guid.TryParse(returnedTenantId, out var parsedTenantId) ? parsedTenantId : Guid.NewGuid(),
                MicrosoftTenantId = returnedTenantId,
                OrganizationName = $"Tenant {returnedTenantId[..Math.Min(8, returnedTenantId.Length)]}"
            };
            _dbContext.Tenants.Add(tenant);
        }
        else
        {
            tenant.OrganizationName = $"Tenant {returnedTenantId[..Math.Min(8, returnedTenantId.Length)]}";
            tenant.IsActive = true;
        }

        pending.UsedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Ok(new { tenant.Id, tenant.MicrosoftTenantId, tenant.OrganizationName });
    }

    private string ResolveAdminConsentRedirectUri()
    {
        var configured = _configuration["AdminConsent:RedirectUri"];
        if (!string.IsNullOrWhiteSpace(configured))
        {
            return configured;
        }

        var origin = Request.Headers.Origin.FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(origin))
        {
            return $"{origin.TrimEnd('/')}/admin-callback";
        }

        return $"{Request.Scheme}://{Request.Host}/admin-callback";
    }

    private static string CreateState()
    {
        Span<byte> bytes = stackalloc byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
