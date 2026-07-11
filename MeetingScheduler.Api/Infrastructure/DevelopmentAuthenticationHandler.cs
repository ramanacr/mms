using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace MeetingScheduler.Api.Infrastructure;

public sealed class DevelopmentAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Development";
    public const string TenantId = "00000000-0000-0000-0000-000000000001";
    public const string AdminEmail = "dev.admin@localhost";
    public const string UserEmail = "dev.user@localhost";
    public const string RoleHeader = "X-Dev-Role";

    public DevelopmentAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var isUser = string.Equals(Request.Headers[RoleHeader].FirstOrDefault(), "user", StringComparison.OrdinalIgnoreCase);
        var email = isUser ? UserEmail : AdminEmail;
        var displayName = isUser ? "Development User" : "Development Admin";
        var roles = isUser ? new[] { "OrgUser" } : new[] { "OrgAdmin", "OrgUser" };

        var claims = new List<Claim>
        {
            new Claim("tid", TenantId),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Upn, email),
            new Claim("name", displayName)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
