using Microsoft.AspNetCore.Mvc;
using MyApp_api.Services;
using MyApp_api.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace MyApp_api.Controllers;

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
        // TODO: Get user ID from JWT/AUTH context
        // var userId = Guid.Parse("08de8d6e-5f8c-4680-8d31-e8ead4ba73ae"); // temporary

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized("Invalid or missing user id claim.");
        
        var result = await _service.CreateAsync(dto, userId);
        return Ok(result);
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
        var properties = await _service.GetAllAsync(minPrice, maxPrice, title, district, city, propertyType, bedrooms, bathrooms, minSquareFeet, maxSquareFeet, page, pageSize);

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
