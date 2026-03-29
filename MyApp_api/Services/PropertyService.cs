using MyApp_api.Models.Entities;
using MyApp_api.Models.DTOs;
using MyApp_api.Data;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore;

namespace MyApp_api.Services;

public class PropertyService : IPropertyService
{
    private readonly AppDbContext _context;
    public PropertyService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PropertyResponseDto> CreateAsync(CreatePropertyDto dto, Guid userId)
    {
        // 1. validation
        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new Exception("Title is required");
        if (dto.Price <= 0)
            throw new Exception("Price must be greater than zero");

        // 2. Map DTO -> Entity
        var property = new Property
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            Price = dto.Price,
            OwnerId = userId,
            CreatedAt = DateTime.UtcNow
        };
        // 3. Save to database
        _context.Properties.Add(property);
        await _context.SaveChangesAsync();

        // 4. Respond: Map Entity -> DTO
        return new PropertyResponseDto
        {
            Id = property.Id,
            Title = property.Title,
            Description = property.Description,
            Price = property.Price,
            OwnerId = property.OwnerId,
            CreatedAt = property.CreatedAt
        };
    }

    public async Task<List<PropertyResponseDto>> GetAllAsync(
    decimal? minPrice,
    decimal? maxPrice,
    string? title
    )
    {
        var query = _context.Properties.AsQueryable();

        // Apply filters conditionally

        if (minPrice.HasValue)
            query = query.Where(p => p.Price >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(p => p.Price <= maxPrice.Value);

        if (!string.IsNullOrWhiteSpace(title))
            query = query.Where(p => p.Title.Contains(title));

        var properties = await query.ToListAsync();

        // Map to DTO
        return properties.Select(p => new PropertyResponseDto
        {
            Id = p.Id,
            Title = p.Title,
            Description = p.Description,
            Price = p.Price,
            OwnerId = p.OwnerId,
            CreatedAt = p.CreatedAt

        }).ToList();
    }



}