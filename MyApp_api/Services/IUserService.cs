namespace MyApp_api.Services;

using MyApp_api.Models.DTOs;

public interface IUserService
{
    Task<UserResponseDto> CreateAsync(CreateUserDto dto);
    Task<List<UserResponseDto>> GetAllAsync();
}