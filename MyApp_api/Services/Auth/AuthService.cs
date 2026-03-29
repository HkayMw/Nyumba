using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyApp_api.Data;
using MyApp_api.Models.DTOs.Auth;
using MyApp_api.Models.Entities;

namespace MyApp_api.Services.Auth;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly PasswordHasher<User> _passwordHasher;

    public AuthService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        var email = dto.Email.Trim().ToLower();

        var exists = await _context.Users.AnyAsync(u => u.Email == email);
        if (exists) throw new Exception("Email already exists");

        var user = new User
        {
            Email = email,
            Role = dto.Role,
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = GenerateToken(user);
        return new AuthResponseDto { Token = token };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var email = dto.Email.Trim().ToLower();

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user is null) throw new Exception("Invalid credintials: Email not found");

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
        if (result == PasswordVerificationResult.Failed)
            throw new Exception("Invalid credentials: Wrong password");

        var token = GenerateToken(user);
        return new AuthResponseDto { Token = token };
    }
}