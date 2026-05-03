using System.ComponentModel.DataAnnotations;

namespace MeetingScheduler.Api.Models;

public sealed class BookingSeries : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid RoomId { get; set; }

    [Required]
    [MaxLength(160)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    [MaxLength(254)]
    public string OrganizerEmail { get; set; } = string.Empty;

    [MaxLength(1024)]
    public string? Attendees { get; set; }

    [MaxLength(120)]
    public string TimeZone { get; set; } = "UTC";

    [MaxLength(256)]
    public string? GraphSeriesId { get; set; }

    [Required]
    [MaxLength(64)]
    public string RecurrenceType { get; set; } = "None";

    [Range(1, 365)]
    public int Interval { get; set; } = 1;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public MeetingRoom Room { get; set; } = null!;
    public ICollection<BookingInstance> Instances { get; set; } = [];
}
