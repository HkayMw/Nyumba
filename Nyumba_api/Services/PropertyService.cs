using Nyumba_api.Models.Entities;
using Nyumba_api.Models.DTOs;
using Nyumba_api.Data;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore;

namespace Nyumba_api.Services;

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
            Address = dto.Address,
            City = dto.City,
            District = dto.District,
            PostalCode = dto.PostalCode,
            PropertyType = dto.PropertyType,
            Bedrooms = dto.Bedrooms,
            Bathrooms = dto.Bathrooms,
            SquareFeet = dto.SquareFeet,
            IsAvailable = true,
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

    public async Task<List<PropertyResponseDto>> GetAllAsync(
        decimal? minPrice,
        decimal? maxPrice,
        string? title,
        string? city,
        string? district,
        string? propertyType,
        int? bedrooms,
        int? bathrooms,
        decimal? minSquareFeet,
        decimal? maxSquareFeet,
        int page,
        int pageSize
    )
    {
        var query = _context.Properties.AsQueryable();

        // Apply price filters
        if (minPrice.HasValue)
            query = query.Where(p => p.Price >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(p => p.Price <= maxPrice.Value);

        // Apply title/description search
        if (!string.IsNullOrWhiteSpace(title))
            query = query.Where(p => (p.Title ?? "").Contains(title) || (p.Description ?? "").Contains(title));

        // Apply location filters
        if (!string.IsNullOrWhiteSpace(city))
            query = query.Where(p => (p.City ?? "").Contains(city));

        // Apply property type filter
        if (!string.IsNullOrWhiteSpace(propertyType))
            query = query.Where(p => p.PropertyType == propertyType);

        // Apply bedroom/bathroom filters
        if (bedrooms.HasValue)
            query = query.Where(p => p.Bedrooms == bedrooms.Value);

        if (bathrooms.HasValue)
            query = query.Where(p => p.Bathrooms == bathrooms.Value);

        // Apply square footage filters
        if (minSquareFeet.HasValue)
            query = query.Where(p => p.SquareFeet >= minSquareFeet.Value);

        if (maxSquareFeet.HasValue)
            query = query.Where(p => p.SquareFeet <= maxSquareFeet.Value);

        // Only list available properties
        query = query.Where(p => p.IsAvailable);

        // Apply pagination
        var skip = (page - 1) * pageSize;
        query = query.OrderByDescending(p => p.CreatedAt)
                     .Skip(skip)
                     .Take(pageSize);

        var properties = await query.ToListAsync();

        // Map to DTO
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

    public async Task<PropertyResponseDto?> GetByIdAsync(Guid id)
    {
        var property = await _context.Properties.FirstOrDefaultAsync(p => p.Id == id);

        if (property is null)
            return null;

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
}
