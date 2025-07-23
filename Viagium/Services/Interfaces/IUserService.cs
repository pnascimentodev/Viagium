using Viagium.EntitiesDTO.User;
using Viagium.Models;
using Viagium.EntitiesDTO.Auth;


namespace Viagium.Services;

public interface IUserService
{
    Task<UserDTO> AddAsync(UserCreateDTO userCreateDto, string password);
    Task<UserDTO?> GetByIdAsync(int id);
    Task<List<UserDTO>> GetAllAsync();
    Task UpdateAsync(UserUpdateDto userUpdateDto, string password);
    Task<LoginResponseDTO> LoginAsync(LoginRequestDTO loginRequest);
    Task<User> DesativateAsync(int id);
    Task<UserDTO> ActivateAsync(int id);
}