using Nyumba_api.Models.Entities;
using Nyumba_api.Models.DTOs;
using Nyumba_api.Data;
using Microsoft.EntityFrameworkCore;
using Nyumba_api.Infrastructure.Errors;

namespace Nyumba_api.Services;

public class PropertyService : IPropertyService
{
    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };

    private const long MaxImageBytes = 5 * 1024 * 1024;

    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public PropertyService(AppDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    public async Task<PropertyResponseDto> CreateAsync(CreatePropertyDto dto, Guid userId)
    {
        // 1. validation
        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new BadRequestException("Title is required");
        if (dto.Price <= 0)
            throw new BadRequestException("Price must be greater than zero");

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
        return MapToResponseDto(property);
    }

    public async Task<PropertyResponseDto> UpdateAsync(Guid id, UpdatePropertyDto dto, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new BadRequestException("Title is required");
        if (dto.Price <= 0)
            throw new BadRequestException("Price must be greater than zero");

        var property = await GetOwnedPropertyAsync(id, userId);

        property.Title = dto.Title;
        property.Description = dto.Description;
        property.Price = dto.Price;
        property.Address = dto.Address;
        property.City = dto.City;
        property.District = dto.District;
        property.PostalCode = dto.PostalCode;
        property.PropertyType = dto.PropertyType;
        property.Bedrooms = dto.Bedrooms;
        property.Bathrooms = dto.Bathrooms;
        property.SquareFeet = dto.SquareFeet;
        property.IsAvailable = dto.IsAvailable;

        await _context.SaveChangesAsync();
        return MapToResponseDto(property);
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var property = await GetOwnedPropertyAsync(id, userId);

        var hasActiveBookings = await _context.Bookings.AnyAsync(b =>
            b.PropertyId == id &&
            (b.Status == BookingStatuses.Pending || b.Status == BookingStatuses.Confirmed));
        if (hasActiveBookings)
            throw new ConflictException("Cannot delete a property with pending or confirmed bookings.");

        _context.Properties.Remove(property);
        await _context.SaveChangesAsync();
    }

    public async Task<PropertyImageResponseDto> UploadImageAsync(Guid id, IFormFile? file, string? caption, Guid userId)
    {
        if (file is null)
            throw new BadRequestException("Image file is required.");
        if (file.Length == 0)
            throw new BadRequestException("Image file is required.");
        if (file.Length > MaxImageBytes)
            throw new BadRequestException("Image file must be 5MB or smaller.");

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedImageExtensions.Contains(extension))
            throw new BadRequestException("Image file must be .jpg, .jpeg, .png, or .webp.");

        await GetOwnedPropertyAsync(id, userId);

        var webRoot = _environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRoot))
            webRoot = Path.Combine(_environment.ContentRootPath, "wwwroot");

        var uploadDirectory = Path.Combine(webRoot, "uploads", "properties", id.ToString());
        Directory.CreateDirectory(uploadDirectory);

        var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var filePath = Path.Combine(uploadDirectory, fileName);
        await using (var stream = File.Create(filePath))
        {
            await file.CopyToAsync(stream);
        }

        var image = new PropertyImage
        {
            Id = Guid.NewGuid(),
            PropertyId = id,
            Url = $"/uploads/properties/{id}/{fileName}",
            Caption = caption,
            CreatedAt = DateTime.UtcNow
        };

        _context.PropertyImages.Add(image);
        await _context.SaveChangesAsync();

        return MapToImageResponseDto(image);
    }

    public async Task RemoveImageAsync(Guid propertyId, Guid imageId, Guid userId)
    {
        await GetOwnedPropertyAsync(propertyId, userId);

        var image = await _context.PropertyImages.FirstOrDefaultAsync(i => i.Id == imageId && i.PropertyId == propertyId);
        if (image is null)
            throw new NotFoundException($"Image {imageId} not found.");

        DeleteImageFileIfLocal(image.Url);

        _context.PropertyImages.Remove(image);
        await _context.SaveChangesAsync();
    }

    public async Task<List<PropertyResponseDto>> GetAllAsync(
        decimal? minPrice,
        decimal? maxPrice,
        string? title,
        string? district,
        string? city,
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

        query = ApplyFilters(
            query,
            minPrice,
            maxPrice,
            title,
            district,
            city,
            propertyType,
            bedrooms,
            bathrooms,
            minSquareFeet,
            maxSquareFeet);

        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        // Apply pagination
        var skip = (page - 1) * pageSize;
        query = query.OrderByDescending(p => p.CreatedAt)
                     .Skip(skip)
                     .Take(pageSize);

        var properties = await query
            .Include(p => p.Images)
            .ToListAsync();

        // Map to DTO
        return properties.Select(MapToResponseDto).ToList();
    }

    internal static IQueryable<Property> ApplyFilters(
        IQueryable<Property> query,
        decimal? minPrice,
        decimal? maxPrice,
        string? title,
        string? district,
        string? city,
        string? propertyType,
        int? bedrooms,
        int? bathrooms,
        decimal? minSquareFeet,
        decimal? maxSquareFeet
    )
    {
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

        if (!string.IsNullOrWhiteSpace(district))
            query = query.Where(p => (p.District ?? "").Contains(district));

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

        return query;
    }

    public async Task<PropertyResponseDto?> GetByIdAsync(Guid id)
    {
        var property = await _context.Properties
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (property is null)
            return null;

        return MapToResponseDto(property);
    }

    private static PropertyResponseDto MapToResponseDto(Property property)
    {
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
            CreatedAt = property.CreatedAt,
            Images = property.Images
                .OrderBy(image => image.CreatedAt)
                .Select(MapToImageResponseDto)
                .ToList()
        };
    }

    private async Task<Property> GetOwnedPropertyAsync(Guid propertyId, Guid userId)
    {
        var property = await _context.Properties
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == propertyId);
        if (property is null)
            throw new NotFoundException($"Property {propertyId} not found.");
        if (property.OwnerId != userId)
            throw new ForbiddenException("You can only manage properties that you own.");

        return property;
    }

    private static PropertyImageResponseDto MapToImageResponseDto(PropertyImage image)
    {
        return new PropertyImageResponseDto
        {
            Id = image.Id,
            PropertyId = image.PropertyId,
            Url = image.Url,
            Caption = image.Caption,
            CreatedAt = image.CreatedAt
        };
    }

    private void DeleteImageFileIfLocal(string url)
    {
        if (!url.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
            return;

        var webRoot = _environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRoot))
            webRoot = Path.Combine(_environment.ContentRootPath, "wwwroot");

        var relativePath = url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var filePath = Path.Combine(webRoot, relativePath);
        if (File.Exists(filePath))
            File.Delete(filePath);
    }
}
