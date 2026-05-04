namespace Nyumba_api.Models.Entities;

public class Booking
{
    public Guid Id { get; set; }
    public Guid PropertyId { get; set; }
    public Guid UserId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = BookingStatuses.Pending;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public Property Property { get; set; } = null!;
    public User User { get; set; } = null!;
}
