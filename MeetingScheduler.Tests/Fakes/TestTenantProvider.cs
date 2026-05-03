using MeetingScheduler.Api.Infrastructure;

namespace MeetingScheduler.Tests.Fakes;

public sealed class TestTenantProvider : ITenantProvider
{
    public Guid TenantId { get; set; } = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public string MicrosoftTenantId => TenantId.ToString();
    public string UserEmail { get; set; } = "organizer@contoso.com";
    public bool HasTenant => TenantId != Guid.Empty;
}
