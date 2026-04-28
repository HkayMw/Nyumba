using System.ComponentModel.DataAnnotations;

namespace Nyumba_api.Models.DTOs.Admin;

public class UpdateUserRoleDto
{
    [Required]
    [RegularExpression("Admin|Landlord|Agent|User", ErrorMessage = "Invalid role")]
    public string NewRole { get; set; }
}
