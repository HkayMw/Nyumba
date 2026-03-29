using Microsoft.AspNetCore.Mvc;
using MyApp_api.Models.DTOs;
using MyApp_api.Services;

namespace MyApp_api.Controllers;

[ApiController]
[Route("api/[controller]")]

public class UserController : ControllerBase
{
    private readonly IUserService _service;

    public UserController(IUserService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserDto dto)
    {
        var user = await _service.CreateAsync(dto);
        return Ok(user);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _service.GetAllAsync();
        return Ok(users);
    }
}