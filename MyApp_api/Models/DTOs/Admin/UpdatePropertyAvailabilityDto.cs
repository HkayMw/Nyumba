using System.ComponentModel.DataAnnotations;

namespace MyApp_api.Models.DTOs.Admin;

public class UpdatePropertyAvailabilityDto
{
    [Required]
    public bool IsAvailable { get; set; }
}
