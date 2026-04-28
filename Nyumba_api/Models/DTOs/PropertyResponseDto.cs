namespace Nyumba_api.Models.DTOs;

public class PropertyResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
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

    // Status
    public bool IsAvailable { get; set; }

    // Ownership & Metadata
    public Guid OwnerId { get; set; }
    public DateTime CreatedAt { get; set; }
}