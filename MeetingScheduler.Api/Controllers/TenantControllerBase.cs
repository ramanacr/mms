using MeetingScheduler.Api.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeetingScheduler.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public abstract class TenantControllerBase : ControllerBase
{
    protected ITenantProvider TenantProvider { get; }

    protected TenantControllerBase(ITenantProvider tenantProvider)
    {
        TenantProvider = tenantProvider;
    }

    protected Guid CurrentTenantId => TenantProvider.TenantId;
    protected string CurrentUserEmail => TenantProvider.UserEmail;
}
