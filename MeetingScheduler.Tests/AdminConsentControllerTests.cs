using MeetingScheduler.Api.Controllers;
using MeetingScheduler.Api.Data;
using MeetingScheduler.Api.Dtos;
using MeetingScheduler.Api.Models;
using MeetingScheduler.Tests.Fakes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace MeetingScheduler.Tests;

public sealed class AdminConsentControllerTests
{
    private static readonly Guid TenantId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    [Fact]
    public void StartAdminConsentRequiresAuthentication()
    {
        var method = typeof(TenantsController).GetMethod(nameof(TenantsController.StartAdminConsent));

        Assert.NotNull(method);
        Assert.Contains(method!.GetCustomAttributes<AuthorizeAttribute>(), _ => true);
    }

    [Fact]
    public async Task StartAdminConsentCreatesPendingStateForCurrentTenant()
    {
        var (controller, db) = CreateController();

        var result = await controller.StartAdminConsent(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<AdminConsentStartResponse>(ok.Value);
        var pending = await db.PendingAdminConsents.SingleAsync();
        Assert.Contains("https://login.microsoftonline.com/common/adminconsent", response.ConsentUrl);
        Assert.Contains($"state={Uri.EscapeDataString(pending.State)}", response.ConsentUrl);
        Assert.Equal(TenantId.ToString(), pending.ExpectedMicrosoftTenantId);
        Assert.Equal("organizer@contoso.com", pending.RequestedByEmail);
        Assert.True(pending.ExpiresAt > DateTime.UtcNow);
        Assert.Null(pending.UsedAt);
    }

    [Theory]
    [InlineData("", true)]
    [InlineData("missing-state", true)]
    [InlineData("valid-state", false)]
    public async Task CompleteAdminConsentRejectsInvalidStateOrDeniedConsent(string state, bool granted)
    {
        var (controller, db) = CreateController();
        if (state == "valid-state")
        {
            db.PendingAdminConsents.Add(PendingConsent(state));
            await db.SaveChangesAsync();
        }

        var result = await controller.CompleteAdminConsent(
            new AdminConsentCompleteRequest(TenantId.ToString(), granted, state),
            CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Empty(db.Tenants);
    }

    [Fact]
    public async Task CompleteAdminConsentRejectsExpiredState()
    {
        var (controller, db) = CreateController();
        db.PendingAdminConsents.Add(PendingConsent("expired-state", expiresAt: DateTime.UtcNow.AddMinutes(-1)));
        await db.SaveChangesAsync();

        var result = await controller.CompleteAdminConsent(
            new AdminConsentCompleteRequest(TenantId.ToString(), true, "expired-state"),
            CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Empty(db.Tenants);
    }

    [Fact]
    public async Task CompleteAdminConsentRejectsReplayedState()
    {
        var (controller, db) = CreateController();
        db.PendingAdminConsents.Add(PendingConsent("used-state", usedAt: DateTime.UtcNow));
        await db.SaveChangesAsync();

        var result = await controller.CompleteAdminConsent(
            new AdminConsentCompleteRequest(TenantId.ToString(), true, "used-state"),
            CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Empty(db.Tenants);
    }

    [Fact]
    public async Task CompleteAdminConsentRejectsTenantMismatch()
    {
        var (controller, db) = CreateController();
        db.PendingAdminConsents.Add(PendingConsent("tenant-state"));
        await db.SaveChangesAsync();

        var result = await controller.CompleteAdminConsent(
            new AdminConsentCompleteRequest(Guid.NewGuid().ToString(), true, "tenant-state"),
            CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Empty(db.Tenants);
    }

    [Fact]
    public async Task CompleteAdminConsentCreatesTenantForValidState()
    {
        var (controller, db) = CreateController();
        db.PendingAdminConsents.Add(PendingConsent("create-state"));
        await db.SaveChangesAsync();

        var result = await controller.CompleteAdminConsent(
            new AdminConsentCompleteRequest(TenantId.ToString(), true, "create-state"),
            CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var tenant = await db.Tenants.SingleAsync();
        var pending = await db.PendingAdminConsents.SingleAsync();
        Assert.NotNull(ok.Value);
        Assert.Equal(TenantId.ToString(), tenant.MicrosoftTenantId);
        Assert.True(tenant.IsActive);
        Assert.NotNull(pending.UsedAt);
    }

    [Fact]
    public async Task CompleteAdminConsentReactivatesExistingTenantForValidState()
    {
        var (controller, db) = CreateController();
        db.PendingAdminConsents.Add(PendingConsent("reactivate-state"));
        db.Tenants.Add(new Tenant
        {
            MicrosoftTenantId = TenantId.ToString(),
            OrganizationName = "Existing tenant",
            IsActive = false
        });
        await db.SaveChangesAsync();

        var result = await controller.CompleteAdminConsent(
            new AdminConsentCompleteRequest(TenantId.ToString(), true, "reactivate-state"),
            CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
        var tenant = await db.Tenants.SingleAsync();
        Assert.True(tenant.IsActive);
        Assert.Equal("Tenant bbbbbbbb", tenant.OrganizationName);
    }

    private static (TenantsController Controller, AppDbContext Db) CreateController()
    {
        var tenant = new TestTenantProvider
        {
            TenantId = TenantId,
            UserEmail = "organizer@contoso.com"
        };
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new AppDbContext(options, tenant);
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AzureAd:ClientId"] = "client-id",
                ["AdminConsent:RedirectUri"] = "https://scheduler.example.com/admin-callback"
            })
            .Build();
        var controller = new TenantsController(db, tenant, config)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        return (controller, db);
    }

    private static PendingAdminConsent PendingConsent(
        string state,
        DateTime? expiresAt = null,
        DateTime? usedAt = null) =>
        new()
        {
            State = state,
            ExpectedMicrosoftTenantId = TenantId.ToString(),
            RequestedByEmail = "organizer@contoso.com",
            ExpiresAt = expiresAt ?? DateTime.UtcNow.AddMinutes(15),
            UsedAt = usedAt
        };
}
