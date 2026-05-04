
namespace Nyumba_api.Services;

using Nyumba_api.Models.DTOs;

public interface IPropertyService

{
    Task<PropertyResponseDto> CreateAsync(CreatePropertyDto dto, Guid userId);
    Task<PropertyResponseDto> UpdateAsync(Guid id, UpdatePropertyDto dto, Guid userId);
    Task DeleteAsync(Guid id, Guid userId);
    Task<PropertyImageResponseDto> UploadImageAsync(Guid id, IFormFile? file, string? caption, Guid userId);
    Task RemoveImageAsync(Guid propertyId, Guid imageId, Guid userId);

    Task<List<PropertyResponseDto>> GetAllAsync(
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
    );

    Task<PropertyResponseDto?> GetByIdAsync(Guid id);
}
