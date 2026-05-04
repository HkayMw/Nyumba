namespace Nyumba_api.Models.Entities;

public class PropertyImage
{
    public Guid Id { get; set; }
    public Guid PropertyId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public DateTime CreatedAt { get; set; }
    public Property Property { get; set; } = null!;
}
