using System.ComponentModel.DataAnnotations;

namespace MyApp_api.Models.DTOs.Auth;

public class RegisterDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    [Required]
    [MinLength(6)]
    public string Password { get; set; }
    [Required]
    [RegularExpression("Admin|Landlord|Agent|User", ErrorMessage = "Invalid role")]
    public string Role { get; set; }
}