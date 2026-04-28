using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Nyumba_api.Services;
using Nyumba_api.Models.DTOs;
using Nyumba_api.Models.DTOs.Admin;

namespace Nyumba_api.Controllers.Admin;

/// <summary>
/// Admin management endpoints for user and property administration.
/// All endpoints require Admin role authorization.
/// </summary>
[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IAdminService _adminService;

    public AdminController(IUserService userService, IAdminService adminService)
    {
        _userService = userService;
        _adminService = adminService;
    }

    // User Management

    /// <summary>
    /// Get all users with pagination.
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20)</param>
    /// <returns>List of users</returns>
    [HttpGet("users")]
    [ProducesResponseType(typeof(List<UserResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var users = await _adminService.GetAllUsersAsync(page, pageSize);
        return Ok(users);
    }

    /// <summary>
    /// Update a user's role.
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="dto">Update role request body</param>
    /// <returns>Updated user</returns>
    [HttpPut("users/{id}/role")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateUserRole(Guid id, [FromBody] UpdateUserRoleDto dto)
    {
        try
        {
            var result = await _adminService.UpdateUserRoleAsync(id, dto.NewRole);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Delete a user. User cannot have active properties.
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>Success message</returns>
    [HttpDelete("users/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        try
        {
            await _adminService.DeleteUserAsync(id);
            return Ok(new { message = $"User {id} deleted successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // Property Management

    /// <summary>
    /// Get all properties (including unavailable) with pagination.
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20)</param>
    /// <returns>List of all properties</returns>
    [HttpGet("properties")]
    [ProducesResponseType(typeof(List<PropertyResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllProperties([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var properties = await _adminService.GetAllPropertiesAsync(page, pageSize);
        return Ok(properties);
    }

    /// <summary>
    /// Update property availability (approve/reject listing).
    /// </summary>
    /// <param name="id">Property ID</param>
    /// <param name="dto">Availability update request body</param>
    /// <returns>Updated property</returns>
    [HttpPut("properties/{id}/availability")]
    [ProducesResponseType(typeof(PropertyResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdatePropertyAvailability(Guid id, [FromBody] UpdatePropertyAvailabilityDto dto)
    {
        try
        {
            var result = await _adminService.UpdatePropertyAvailabilityAsync(id, dto.IsAvailable);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Delete a property listing.
    /// </summary>
    /// <param name="id">Property ID</param>
    /// <returns>Success message</returns>
    [HttpDelete("properties/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteProperty(Guid id)
    {
        try
        {
            await _adminService.DeletePropertyAsync(id);
            return Ok(new { message = $"Property {id} deleted successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
