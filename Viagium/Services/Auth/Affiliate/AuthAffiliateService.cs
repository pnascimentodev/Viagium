using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Viagium.Configurations;
using Viagium.EntitiesDTO.Auth;
using Viagium.Repository.Interface;

namespace Viagium.Services.Auth.Affiliate;

public class AuthAffiliateService : IAuthAffiliateService
{
    private readonly IAffiliateRepository _affiliateRepository;
    private readonly JwtSettings _jwtSettings;

    public AuthAffiliateService(IAffiliateRepository affiliateRepository, IOptions<JwtSettings> jwtOptions)
    {
        _affiliateRepository = affiliateRepository;
        _jwtSettings = jwtOptions.Value;
    }

    public async Task<AffiliateLoginResponseDTO> LoginAsync(LoginRequestDTO loginRequest)
    {
        if (string.IsNullOrWhiteSpace(loginRequest.Email) || string.IsNullOrWhiteSpace(loginRequest.Password))
            throw new ArgumentException("Email e senha s�o obrigat�rios.");

        var affiliate = await _affiliateRepository.GetByEmailAsync(loginRequest.Email);
        if (affiliate == null || !affiliate.IsActive || affiliate.DeletedAt != null)
            throw new UnauthorizedAccessException("Usu�rio ou senha inv�lidos.");

        if (!PasswordHelper.VerifyPassword(loginRequest.Password, affiliate.HashPassword))
            throw new UnauthorizedAccessException("Usu�rio ou senha inv�lidos.");

        var token = GenerateToken(affiliate);
        return new AffiliateLoginResponseDTO
        {
            Id = affiliate.AffiliateId.ToString(),
            Token = token
        };
    }

    /// Gera um token JWT para o usu�rio autenticado.
    public string GenerateToken(Models.Affiliate affiliate)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, affiliate.AffiliateId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, affiliate.Email),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}