using System.ComponentModel.DataAnnotations;

namespace MeetingScheduler.Api.Models;

public sealed class PendingAdminConsent
{
    [Key]
    [MaxLength(128)]
    public string State { get; set; } = string.Empty;

    [Required]
    [MaxLength(64)]
    public string ExpectedMicrosoftTenantId { get; set; } = string.Empty;

    [Required]
    [MaxLength(254)]
    public string RequestedByEmail { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }
}
