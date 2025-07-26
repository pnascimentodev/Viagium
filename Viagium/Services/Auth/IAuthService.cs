using Viagium.EntitiesDTO.Auth;
using Viagium.Models;

namespace Viagium.Services.Auth;

public interface IAuthService
{
    Task<LoginResponseDTO> LoginWithRoleAsync(LoginRequestDTO loginRequest, Role requiredRole);
    string GenerateJwtToken(User user);
}