using System.ComponentModel.DataAnnotations;

namespace MeetingScheduler.Api.Models;

public sealed class MeetingRoom : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(80)]
    public string Floor { get; set; } = string.Empty;

    [Required]
    [MaxLength(160)]
    public string Location { get; set; } = string.Empty;

    [Range(1, 1000)]
    public int Capacity { get; set; }

    [MaxLength(500)]
    public string? Amenities { get; set; }

    [EmailAddress]
    [MaxLength(254)]
    public string? ExchangeEmail { get; set; }

    public bool IsActive { get; set; } = true;

    public Tenant? Tenant { get; set; }
    public ICollection<BookingSeries> BookingSeries { get; set; } = [];
    public ICollection<BookingInstance> BookingInstances { get; set; } = [];
}
