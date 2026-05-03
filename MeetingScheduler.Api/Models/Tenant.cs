using System.ComponentModel.DataAnnotations;

namespace MeetingScheduler.Api.Models;

public sealed class Tenant
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(64)]
    public string MicrosoftTenantId { get; set; } = string.Empty;

    [Required]
    [MaxLength(160)]
    public string OrganizationName { get; set; } = string.Empty;

    [MaxLength(160)]
    public string? CustomDomain { get; set; }

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    public ICollection<MeetingRoom> Rooms { get; set; } = [];
}
