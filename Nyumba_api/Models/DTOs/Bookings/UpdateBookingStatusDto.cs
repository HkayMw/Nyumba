using System.ComponentModel.DataAnnotations;

namespace Nyumba_api.Models.DTOs.Bookings;

public class UpdateBookingStatusDto
{
    [Required]
    public string Status { get; set; } = string.Empty;
}
