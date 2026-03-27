
namespace MyApp_api.Services;

using MyApp_api.Models.DTOs;

public interface IPropertyService

{
    Task<PropertyResponseDto> CreateAsync(CreatePropertyDto dto, Guid userId);
    // Task CreateAsync(CreatePropertyDto dto, Func<Guid> userId);
}