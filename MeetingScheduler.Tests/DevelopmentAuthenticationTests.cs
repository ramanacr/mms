using MeetingScheduler.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net;
using System.Net.Http.Json;

namespace MeetingScheduler.Tests;

public sealed class DevelopmentAuthenticationTests
{
    [Fact]
    public async Task DevelopmentAllowsProfileWithoutMsalToken()
    {
        await using var factory = CreateFactory("Development");
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/profile/me");

        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.OK, $"Expected OK but got {response.StatusCode}: {body}");
        var profile = await response.Content.ReadFromJsonAsync<ProfileResponse>();
        Assert.NotNull(profile);
        Assert.Equal("00000000-0000-0000-0000-000000000001", profile!.TenantId);
        Assert.Equal("dev.admin@localhost", profile.Email);
        Assert.Contains("OrgAdmin", profile.Roles);
    }

    [Fact]
    public async Task DevelopmentUserRoleHeaderReturnsNonAdminProfile()
    {
        await using var factory = CreateFactory("Development");
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Dev-Role", "user");

        var response = await client.GetAsync("/api/profile/me");

        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.OK, $"Expected OK but got {response.StatusCode}: {body}");
        var profile = await response.Content.ReadFromJsonAsync<ProfileResponse>();
        Assert.NotNull(profile);
        Assert.Equal("dev.user@localhost", profile!.Email);
        Assert.Contains("OrgUser", profile.Roles);
        Assert.DoesNotContain("OrgAdmin", profile.Roles);
    }

    [Fact]
    public async Task DevelopmentSeedsRoomsForScheduling()
    {
        await using var factory = CreateFactory("Development");
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/rooms");

        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.OK, $"Expected OK but got {response.StatusCode}: {body}");
        var rooms = await response.Content.ReadFromJsonAsync<RoomResponse[]>();
        Assert.NotNull(rooms);
        Assert.Contains(rooms!, room => room.Name == "Conference Room A");
    }

    [Fact]
    public async Task ProductionStillRejectsProfileWithoutMsalToken()
    {
        await using var factory = CreateFactory("Production");
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/profile/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private static WebApplicationFactory<Program> CreateFactory(string environment) =>
        new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment(environment);
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<DbContextOptions<AppDbContext>>();
                    services.RemoveAll<DbContextOptions>();
                    services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();

                    services.AddDbContext<AppDbContext>(options =>
                        options.UseInMemoryDatabase($"{environment}-{Guid.NewGuid()}"));
                });
            });

    private sealed record ProfileResponse(string DisplayName, string Email, string TenantId, string[] Roles);
    private sealed record RoomResponse(Guid Id, string Name);
}
