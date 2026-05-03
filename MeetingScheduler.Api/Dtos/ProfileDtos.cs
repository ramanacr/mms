namespace MeetingScheduler.Api.Dtos;

public sealed record ProfileDto(string DisplayName, string Email, string TenantId, string[] Roles);

public sealed record DashboardStatsDto(int TotalRooms, int BookingsToday, bool AdminConsentGranted, bool GraphSyncActive);

public sealed record AdminConsentRequest(string MicrosoftTenantId, string OrganizationName, string? CustomDomain);
