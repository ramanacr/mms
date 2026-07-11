using MeetingScheduler.Api.Data;
using MeetingScheduler.Api.Models;
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

            if (context.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment()
                && tenantProvider.MicrosoftTenantId == DevelopmentAuthenticationHandler.TenantId)
            {
                await EnsureDevelopmentTenantAsync(dbContext, cancellationToken: context.RequestAborted);
                await _next(context);
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

    private static async Task EnsureDevelopmentTenantAsync(AppDbContext dbContext, CancellationToken cancellationToken)
    {
        var developmentTenantId = Guid.Parse(DevelopmentAuthenticationHandler.TenantId);
        var tenant = await dbContext.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.MicrosoftTenantId == DevelopmentAuthenticationHandler.TenantId, cancellationToken);
        var changed = false;

        if (tenant is null)
        {
            tenant = new Tenant
            {
                Id = developmentTenantId,
                MicrosoftTenantId = DevelopmentAuthenticationHandler.TenantId,
                OrganizationName = "Development Tenant"
            };
            dbContext.Tenants.Add(tenant);
            changed = true;
        }

        if (!tenant.IsActive)
        {
            tenant.IsActive = true;
            changed = true;
        }

        if (tenant.Id != developmentTenantId && dbContext.Database.IsRelational())
        {
            await dbContext.BookingInstances.IgnoreQueryFilters().Where(b => b.TenantId == tenant.Id).ExecuteDeleteAsync(cancellationToken);
            await dbContext.BookingSeries.IgnoreQueryFilters().Where(b => b.TenantId == tenant.Id).ExecuteDeleteAsync(cancellationToken);
            await dbContext.MeetingRooms.IgnoreQueryFilters().Where(r => r.TenantId == tenant.Id).ExecuteDeleteAsync(cancellationToken);
            await dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE [Tenants] SET [Id] = {developmentTenantId} WHERE [Id] = {tenant.Id}",
                cancellationToken);
            dbContext.ChangeTracker.Clear();
            tenant = await dbContext.Tenants
                .IgnoreQueryFilters()
                .FirstAsync(t => t.MicrosoftTenantId == DevelopmentAuthenticationHandler.TenantId, cancellationToken);
            changed = false;
        }

        var hasRooms = await dbContext.MeetingRooms
            .IgnoreQueryFilters()
            .AnyAsync(r => r.TenantId == developmentTenantId, cancellationToken);

        if (!hasRooms)
        {
            dbContext.MeetingRooms.AddRange(
                new MeetingRoom
                {
                    TenantId = developmentTenantId,
                    Name = "Conference Room A",
                    Floor = "3",
                    Location = "Floor 3, West Wing",
                    Capacity = 12,
                    Amenities = "Display, whiteboard, Teams console"
                },
                new MeetingRoom
                {
                    TenantId = developmentTenantId,
                    Name = "Focus Studio",
                    Floor = "4",
                    Location = "Floor 4, Quiet Zone",
                    Capacity = 4,
                    Amenities = "Display, acoustic panels"
                },
                new MeetingRoom
                {
                    TenantId = developmentTenantId,
                    Name = "Board Room",
                    Floor = "7",
                    Location = "Floor 7, Executive Suite",
                    Capacity = 18,
                    Amenities = "Dual displays, video conferencing, whiteboard"
                });
            changed = true;
        }

        if (changed)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
