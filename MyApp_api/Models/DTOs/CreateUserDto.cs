using System.ComponentModel.DataAnnotations;

namespace MyApp_api.Models.DTOs;

public class CreateUserDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    [Required]
    [RegularExpression("Admin|Owner", ErrorMessage ="Invalid role")]
    public string Role { get; set; }
}