using Viagium.Models;
using Viagium.EntitiesDTO;
using Viagium.EntitiesDTO.Auth;

namespace Viagium.Services.Interfaces;

public interface IAdminService
{
    Task<User> RegisterAsync(AdminRegisterDTO adminRegisterDto);
    Task<LoginResponseDTO> LoginWithRoleAsync(LoginRequestDTO loginRequest);
    Task<AdminDTO?> GetByIdAsync(int id);
    Task<List<AdminDTO>> GetAllAsync();
    Task<List<AdminDTO>> GetAllActiveAsync();
    Task<AdminDTO> UpdateAsync(int id, AdminUpdateDTO adminUpdateDto);
    Task<User> DesativateUserAsync(int id);
    Task<User> ActivateUserAsync(int id);
}
