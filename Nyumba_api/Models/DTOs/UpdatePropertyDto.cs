using System.ComponentModel.DataAnnotations;

namespace Nyumba_api.Models.DTOs;

public class UpdatePropertyDto
{
    [Required]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero")]
    public decimal Price { get; set; }

    public string? Address { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? PostalCode { get; set; }
    public string? PropertyType { get; set; }
    public int? Bedrooms { get; set; }
    public int? Bathrooms { get; set; }
    public decimal? SquareFeet { get; set; }
    public bool IsAvailable { get; set; } = true;
}
