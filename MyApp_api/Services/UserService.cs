using Microsoft.EntityFrameworkCore;
using MyApp_api.Data;
using MyApp_api.Models.DTOs;
using MyApp_api.Models.Entities;

namespace MyApp_api.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    public UserService(AppDbContext context)
    {
        _context = context;
    }
    public async Task<UserResponseDto> CreateAsync(CreateUserDto dto)
    {
        // 1. validation
        // Normalize
        var email = dto.Email.Trim().ToLower();

        // check uniqueness
        var exists = await _context.Users.AnyAsync(u => u.Email == email);
        if (exists)
            throw new Exception("Email already exists");


        // 2. Map DTO -> Entity
        var user = new User
        {
            Email = dto.Email.Trim().ToLower(),
            Role = dto.Role,
            CreatedAt = DateTime.UtcNow
        };
        // 3. Save to Db
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // 4. Respond: Map Entity -> DTO
        return new UserResponseDto
        {
            Id = user.Id,
            Email = user.Email,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        };

    }

    public async Task<List<UserResponseDto>> GetAllAsync()
    {
        var users = await _context.Users.ToListAsync();

        return users.Select(u => new UserResponseDto
        {
            Id = u.Id,
            Email = u.Email,
            Role = u.Role,
            CreatedAt = u.CreatedAt
        }).ToList();
    }
}