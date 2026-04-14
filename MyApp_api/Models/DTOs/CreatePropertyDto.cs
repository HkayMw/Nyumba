using System.ComponentModel.DataAnnotations;

namespace MyApp_api.Models.DTOs;

public class CreatePropertyDto
{
    [Required]
    public string Title { get; set; }
    
    public string? Description { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero")]
    public decimal Price { get; set; }
    
    // Location
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? PostalCode { get; set; }
    
    // Property Details
    public string? PropertyType { get; set; }
    public int? Bedrooms { get; set; }
    public int? Bathrooms { get; set; }
    public decimal? SquareFeet { get; set; }
}