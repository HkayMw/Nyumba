
namespace Nyumba_api.Services;

using Nyumba_api.Models.DTOs;

public interface IPropertyService

{
    Task<PropertyResponseDto> CreateAsync(CreatePropertyDto dto, Guid userId);

    Task<List<PropertyResponseDto>> GetAllAsync(
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
    );

    Task<PropertyResponseDto?> GetByIdAsync(Guid id);
}
