using Microsoft.AspNetCore.Mvc;
using MyApp_api.Services;
using MyApp_api.Models.DTOs;

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
        var userId = Guid.NewGuid(); //temporary
        var result = await _service.CreateAsync(dto, userId);
        return Ok(result);
    }

    //     [HttpGet]
    //     public async Task<IActionResult> GetAllProperties()
    //     {
    //         var Properties = _service
    //     }
}