using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Nyumba_api.Data;
using Nyumba_api.Models.Authorization;
using Nyumba_api.Models.DTOs;
using Nyumba_api.Models.Entities;

namespace Nyumba_api.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly PasswordHasher<User> _passwordHasher;

    public UserService(AppDbContext context, PasswordHasher<User> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }
    public async Task<UserResponseDto> CreateAsync(CreateUserDto dto)
    {
        // 1. validation
        // Normalize
        var email = dto.Email.Trim().ToLower();

        // check uniqueness
        var exists = await _context.Users.AnyAsync(u => u.Email == email);
        if (exists)
            throw new InvalidOperationException("Email already exists");

        var role = AppRoles.Normalize(dto.Role.Trim());
        if (!AppRoles.IsValid(role))
            throw new InvalidOperationException("Invalid role. Valid roles: Admin, Landlord, Agent, User");


        // 2. Map DTO -> Entity
        var user = new User
        {
            Email = email,
            Role = role,
            CreatedAt = DateTime.UtcNow
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);
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
