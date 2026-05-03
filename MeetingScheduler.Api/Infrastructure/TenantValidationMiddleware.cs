using MeetingScheduler.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace MeetingScheduler.Api.Infrastructure;

public sealed class TenantValidationMiddleware
{
    private readonly RequestDelegate _next;

    public TenantValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext dbContext, ITenantProvider tenantProvider)
    {
        if (context.User.Identity?.IsAuthenticated == true
            && !context.Request.Path.StartsWithSegments("/api/tenants/admin-consent"))
        {
            if (!tenantProvider.HasTenant)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Tenant claim is missing.");
                return;
            }

            var exists = await dbContext.Tenants
                .IgnoreQueryFilters()
                .AnyAsync(t => t.MicrosoftTenantId == tenantProvider.MicrosoftTenantId && t.IsActive);

            if (!exists)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Organization not registered. Please contact an admin.");
                return;
            }
        }

        await _next(context);
    }
}
