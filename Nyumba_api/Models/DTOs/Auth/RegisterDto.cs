using System.ComponentModel.DataAnnotations;
using Nyumba_api.Models.Authorization;

namespace Nyumba_api.Models.DTOs.Auth;

public class RegisterDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
    [Required]
    [RegularExpression(AppRoles.PublicRegistrationRolesPattern, ErrorMessage = "Role must be Landlord, Agent, or User")]
    public string Role { get; set; } = string.Empty;
}
