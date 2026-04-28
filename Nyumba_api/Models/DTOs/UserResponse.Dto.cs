namespace Nyumba_api.Models.DTOs;

public class UserResponseDto
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public DateTime CreatedAt { get; set; }
}