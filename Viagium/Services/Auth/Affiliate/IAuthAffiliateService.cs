using Viagium.EntitiesDTO.Auth;

namespace Viagium.Services.Auth.Affiliate;

public interface IAuthAffiliateService
{
    Task<AffiliateLoginResponseDTO> LoginAsync(LoginRequestDTO loginRequest);
    string GenerateToken(Models.Affiliate affiliate);
    Task<AffiliateLoginResponseDTO> LogoutAsync(string token);
}