namespace MyApp_api.Models.DTOs;

public class PropertyResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public Guid OwnerId { get; set; }
    public DateTime CreatedAt { get; set; }
}