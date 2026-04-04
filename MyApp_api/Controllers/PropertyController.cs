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

    [HttpPost]
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

    [HttpGet]
    public async Task<IActionResult> GetAllProperties(
[FromQuery] decimal? minPrice,
[FromQuery] decimal? maxPrice,
[FromQuery] string? title
    )
    {
        var properties = await _service.GetAllAsync(minPrice, maxPrice, title);

        return Ok(properties);
    }
}