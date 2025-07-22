using Viagium.EntitiesDTO.Auth;

namespace Viagium.Services.Auth;

public interface IAuthService
{
    /// Autentica o usu�rio e retorna um token JWT se v�lido.
    Task<LoginResponseDTO> LoginAsync(LoginRequestDTO loginRequest);
}