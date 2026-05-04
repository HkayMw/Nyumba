using Microsoft.AspNetCore.Mvc;
using Nyumba_api.Services;
using Nyumba_api.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Nyumba_api.Infrastructure.Auth;

namespace Nyumba_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PropertyController : ControllerBase
{
    private readonly IPropertyService _service;

    public PropertyController(IPropertyService service)
    {
        _service = service;
    }

    /// <summary>
    /// Create a new property listing for the authenticated landlord or agent.
    /// </summary>
    /// <param name="dto">Property details.</param>
    /// <returns>The created property.</returns>
    [HttpPost]
    [Authorize(Roles = "Landlord,Agent")]
    [ProducesResponseType(typeof(PropertyResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create(CreatePropertyDto dto)
    {
        var result = await _service.CreateAsync(dto, User.GetUserId());
        return Ok(result);
    }

    /// <summary>
    /// Update a property listing owned by the authenticated landlord or agent.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Landlord,Agent")]
    [ProducesResponseType(typeof(PropertyResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePropertyDto dto)
    {
        var result = await _service.UpdateAsync(id, dto, User.GetUserId());
        return Ok(result);
    }

    /// <summary>
    /// Delete a property listing owned by the authenticated landlord or agent.
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Landlord,Agent")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id, User.GetUserId());
        return Ok(new { message = $"Property {id} deleted successfully." });
    }

    /// <summary>
    /// Upload an image for a property owned by the authenticated landlord or agent.
    /// </summary>
    [HttpPost("{id}/images")]
    [Authorize(Roles = "Landlord,Agent")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(PropertyImageResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadImage(Guid id, [FromForm] UploadImageRequestDto request)
{
    var result = await _service.UploadImageAsync(id, request.File, request.Caption, User.GetUserId());
    return Ok(result);
}

    /// <summary>
    /// Remove an image from a property owned by the authenticated landlord or agent.
    /// </summary>
    [HttpDelete("{id}/images/{imageId}")]
    [Authorize(Roles = "Landlord,Agent")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveImage(Guid id, Guid imageId)
    {
        await _service.RemoveImageAsync(id, imageId, User.GetUserId());
        return Ok(new { message = $"Image {imageId} deleted successfully." });
    }

    /// <summary>
    /// Get all available properties with optional filtering and pagination.
    /// </summary>
    /// <returns>A filtered list of available properties.</returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<PropertyResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllProperties(
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] string? title,
        [FromQuery] string? district,
        [FromQuery] string? city,
        [FromQuery] string? propertyType,
        [FromQuery] int? bedrooms,
        [FromQuery] int? bathrooms,
        [FromQuery] decimal? minSquareFeet,
        [FromQuery] decimal? maxSquareFeet,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20
    )
    {
        var properties = await _service.GetAllAsync(
            minPrice: minPrice,
            maxPrice: maxPrice,
            title: title,
            district: district,
            city: city,
            propertyType: propertyType,
            bedrooms: bedrooms,
            bathrooms: bathrooms,
            minSquareFeet: minSquareFeet,
            maxSquareFeet: maxSquareFeet,
            page: page,
            pageSize: pageSize);

        return Ok(properties);
    }

    /// <summary>
    /// Get a property by its identifier.
    /// </summary>
    /// <param name="id">Property ID.</param>
    /// <returns>The matching property.</returns>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PropertyResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var property = await _service.GetByIdAsync(id);

        if (property is null)
            return NotFound($"Property with id {id} not found.");

        return Ok(property);
    }
}
