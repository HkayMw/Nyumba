using Nyumba_api.Models.DTOs;

namespace Nyumba_api.Services;

public interface IAdminService
{
    // User Management
    Task<List<UserResponseDto>> GetAllUsersAsync(int page, int pageSize);
    Task<UserResponseDto> UpdateUserRoleAsync(Guid userId, string newRole);
    Task DeleteUserAsync(Guid userId);

    // Property Management
    Task<List<PropertyResponseDto>> GetAllPropertiesAsync(int page, int pageSize);
    Task<PropertyResponseDto> UpdatePropertyAvailabilityAsync(Guid propertyId, bool isAvailable);
    Task DeletePropertyAsync(Guid propertyId);
}
