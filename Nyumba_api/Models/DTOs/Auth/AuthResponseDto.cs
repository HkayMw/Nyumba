namespace Nyumba_api.Models.DTOs.Auth;

public class AuthResponseDto
{
    public string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
}