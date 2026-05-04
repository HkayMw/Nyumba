using System.ComponentModel.DataAnnotations;

namespace Nyumba_api.Models.DTOs.Bookings;

public class CreateBookingDto
{
    [Required]
    public Guid PropertyId { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    public string? Notes { get; set; }
}
