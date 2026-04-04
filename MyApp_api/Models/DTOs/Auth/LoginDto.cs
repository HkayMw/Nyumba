using System.ComponentModel.DataAnnotations;

namespace MyApp_api.Models.DTOs.Auth;

public class LoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    [Required]
    [MinLength(6)]
    public string Password { get; set; }
    
}