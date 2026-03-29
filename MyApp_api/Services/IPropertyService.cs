
namespace MyApp_api.Services;

using MyApp_api.Models.DTOs;

public interface IPropertyService

{
    Task<PropertyResponseDto> CreateAsync(CreatePropertyDto dto, Guid userId);
    
    Task<List<PropertyResponseDto>> GetAllAsync(
        decimal? minPrice,
        decimal? maxPrice,
        string? title
    );
}
