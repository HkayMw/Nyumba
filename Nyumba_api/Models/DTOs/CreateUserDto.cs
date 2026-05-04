using System.ComponentModel.DataAnnotations;
using Nyumba_api.Models.Authorization;

namespace Nyumba_api.Models.DTOs;

public class CreateUserDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
    [Required]
    [RegularExpression(AppRoles.AllRolesPattern, ErrorMessage = "Invalid role")]
    public string Role { get; set; } = string.Empty;
}
