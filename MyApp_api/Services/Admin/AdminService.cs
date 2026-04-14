using Microsoft.EntityFrameworkCore;
using MyApp_api.Data;
using MyApp_api.Models.DTOs;
using MyApp_api.Models.Entities;

namespace MyApp_api.Services;

public class AdminService : IAdminService
{
    private readonly AppDbContext _context;

    public AdminService(AppDbContext context)
    {
        _context = context;
    }

    // User Management

    public async Task<List<UserResponseDto>> GetAllUsersAsync(int page, int pageSize)
    {
        var skip = (page - 1) * pageSize;
        var users = await _context.Users
            .OrderByDescending(u => u.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();

        return users.Select(u => new UserResponseDto
        {
            Id = u.Id,
            Email = u.Email,
            Role = u.Role,
            CreatedAt = u.CreatedAt
        }).ToList();
    }

    public async Task<UserResponseDto> UpdateUserRoleAsync(Guid userId, string newRole)
    {
        if (string.IsNullOrWhiteSpace(newRole))
            throw new Exception("Role cannot be empty");

        if (!IsValidRole(newRole))
            throw new Exception("Invalid role. Valid roles: Admin, Landlord, Agent, User");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null)
            throw new Exception($"User {userId} not found");

        user.Role = newRole;
        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        return new UserResponseDto
        {
            Id = user.Id,
            Email = user.Email,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task DeleteUserAsync(Guid userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null)
            throw new Exception($"User {userId} not found");

        // Optionally, check if user owns properties
        var ownedProperties = await _context.Properties.Where(p => p.OwnerId == userId).ToListAsync();
        if (ownedProperties.Count > 0)
            throw new Exception($"Cannot delete user with {ownedProperties.Count} active properties. Delete or reassign properties first.");

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }

    // Property Management

    public async Task<List<PropertyResponseDto>> GetAllPropertiesAsync(int page, int pageSize)
    {
        var skip = (page - 1) * pageSize;
        var properties = await _context.Properties
            .OrderByDescending(p => p.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();

        return properties.Select(p => new PropertyResponseDto
        {
            Id = p.Id,
            Title = p.Title,
            Description = p.Description,
            Price = p.Price,
            Address = p.Address,
            City = p.City,
            District = p.District,
            PostalCode = p.PostalCode,
            PropertyType = p.PropertyType,
            Bedrooms = p.Bedrooms,
            Bathrooms = p.Bathrooms,
            SquareFeet = p.SquareFeet,
            IsAvailable = p.IsAvailable,
            OwnerId = p.OwnerId,
            CreatedAt = p.CreatedAt
        }).ToList();
    }

    public async Task<PropertyResponseDto> UpdatePropertyAvailabilityAsync(Guid propertyId, bool isAvailable)
    {
        var property = await _context.Properties.FirstOrDefaultAsync(p => p.Id == propertyId);
        if (property is null)
            throw new Exception($"Property {propertyId} not found");

        property.IsAvailable = isAvailable;
        _context.Properties.Update(property);
        await _context.SaveChangesAsync();

        return new PropertyResponseDto
        {
            Id = property.Id,
            Title = property.Title,
            Description = property.Description,
            Price = property.Price,
            Address = property.Address,
            City = property.City,
            District = property.District,
            PostalCode = property.PostalCode,
            PropertyType = property.PropertyType,
            Bedrooms = property.Bedrooms,
            Bathrooms = property.Bathrooms,
            SquareFeet = property.SquareFeet,
            IsAvailable = property.IsAvailable,
            OwnerId = property.OwnerId,
            CreatedAt = property.CreatedAt
        };
    }

    public async Task DeletePropertyAsync(Guid propertyId)
    {
        var property = await _context.Properties.FirstOrDefaultAsync(p => p.Id == propertyId);
        if (property is null)
            throw new Exception($"Property {propertyId} not found");

        _context.Properties.Remove(property);
        await _context.SaveChangesAsync();
    }

    private bool IsValidRole(string role)
    {
        var validRoles = new[] { "Admin", "Landlord", "Agent", "User" };
        return validRoles.Contains(role);
    }
}
