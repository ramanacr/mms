using MeetingScheduler.Api.Dtos;
using MeetingScheduler.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MeetingScheduler.Api.Controllers;

public sealed class ProfileController : TenantControllerBase
{
    public ProfileController(ITenantProvider tenantProvider) : base(tenantProvider)
    {
    }

    [HttpGet("me")]
    public ActionResult<ProfileDto> Me()
    {
        var displayName = User.FindFirstValue("name") ?? User.Identity?.Name ?? CurrentUserEmail;
        var roles = User.FindAll(ClaimTypes.Role)
            .Concat(User.FindAll("roles"))
            .Select(r => r.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return Ok(new ProfileDto(displayName, CurrentUserEmail, TenantProvider.MicrosoftTenantId, roles));
    }
}
