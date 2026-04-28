using System.ComponentModel.DataAnnotations;

namespace Nyumba_api.Models.DTOs.Admin;

public class UpdatePropertyAvailabilityDto
{
    [Required]
    public bool IsAvailable { get; set; }
}
