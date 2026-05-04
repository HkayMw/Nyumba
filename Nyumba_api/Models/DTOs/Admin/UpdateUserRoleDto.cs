using System.ComponentModel.DataAnnotations;
using Nyumba_api.Models.Authorization;

namespace Nyumba_api.Models.DTOs.Admin;

public class UpdateUserRoleDto
{
    [Required]
    [RegularExpression(AppRoles.AllRolesPattern, ErrorMessage = "Invalid role")]
    public string NewRole { get; set; } = string.Empty;
}
