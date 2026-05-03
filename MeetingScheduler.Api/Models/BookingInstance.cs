using System.ComponentModel.DataAnnotations;

namespace MeetingScheduler.Api.Models;

public sealed class BookingInstance : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid? SeriesId { get; set; }
    public Guid RoomId { get; set; }

    [Required]
    [MaxLength(160)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    [MaxLength(254)]
    public string OrganizerEmail { get; set; } = string.Empty;

    [MaxLength(1024)]
    public string? Attendees { get; set; }

    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }

    [MaxLength(256)]
    public string? GraphInstanceId { get; set; }

    public BookingSeries? Series { get; set; }
    public MeetingRoom Room { get; set; } = null!;
}
