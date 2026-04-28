using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Nyumba_api.Models.DTOs.Auth;
using Nyumba_api.Services.Auth;

namespace Nyumba_api.Controllers.Auth;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Register a new user account and return a JWT token.
    /// </summary>
    /// <param name="dto">Registration details.</param>
    /// <returns>The authentication token and expiry time.</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var result = await _authService.RegisterAsync(dto);
        return Ok(result);
    }

    /// <summary>
    /// Authenticate an existing user and return a JWT token.
    /// </summary>
    /// <param name="dto">Login credentials.</param>
    /// <returns>The authentication token and expiry time.</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);
        return Ok(result);
    }
}
