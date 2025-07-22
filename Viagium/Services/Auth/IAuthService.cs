using Viagium.EntitiesDTO.Auth;

namespace Viagium.Services.Auth;

public interface IAuthService
{
    /// Autentica o usuário e retorna um token JWT se válido.
    Task<LoginResponseDTO> LoginAsync(LoginRequestDTO loginRequest);
}