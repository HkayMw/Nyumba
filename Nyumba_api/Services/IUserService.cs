namespace Nyumba_api.Services;

using Nyumba_api.Models.DTOs;

public interface IUserService
{
    Task<UserResponseDto> CreateAsync(CreateUserDto dto);
    Task<List<UserResponseDto>> GetAllAsync();
}