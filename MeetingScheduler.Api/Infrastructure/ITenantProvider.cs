using System.Security.Claims;

namespace MeetingScheduler.Api.Infrastructure;

public interface ITenantProvider
{
    Guid TenantId { get; }
    string MicrosoftTenantId { get; }
    string UserEmail { get; }
    bool HasTenant { get; }
}

public sealed class ClaimsTenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _accessor;

    public ClaimsTenantProvider(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public Guid TenantId
    {
        get
        {
            var tid = MicrosoftTenantId;
            return Guid.TryParse(tid, out var parsed) ? parsed : Guid.Empty;
        }
    }

    public string MicrosoftTenantId =>
        _accessor.HttpContext?.User.FindFirstValue("tid")
        ?? _accessor.HttpContext?.User.FindFirstValue("http://schemas.microsoft.com/identity/claims/tenantid")
        ?? string.Empty;

    public string UserEmail =>
        _accessor.HttpContext?.User.FindFirstValue(ClaimTypes.Upn)
        ?? _accessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email)
        ?? _accessor.HttpContext?.User.FindFirstValue("preferred_username")
        ?? string.Empty;

    public bool HasTenant => !string.IsNullOrWhiteSpace(MicrosoftTenantId);
}
