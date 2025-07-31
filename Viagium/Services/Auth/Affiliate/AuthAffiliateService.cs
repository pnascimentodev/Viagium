using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Viagium.Configurations;
using Viagium.EntitiesDTO.Auth;
using Viagium.Repository.Interface;
using Microsoft.Extensions.Logging;

namespace Viagium.Services.Auth.Affiliate;

public class AuthAffiliateService : IAuthAffiliateService
{
    private readonly IAffiliateRepository _affiliateRepository;
    private readonly JwtSettings _jwtSettings;
    private readonly ITokenBlacklistService _tokenBlacklistService;
    private readonly ILogger<AuthAffiliateService> _logger;

    public AuthAffiliateService(IAffiliateRepository affiliateRepository, IOptions<JwtSettings> jwtOptions, ITokenBlacklistService tokenBlacklistService, ILogger<AuthAffiliateService> logger)
    {
        _affiliateRepository = affiliateRepository;
        _jwtSettings = jwtOptions.Value;
        _tokenBlacklistService = tokenBlacklistService;
        _logger = logger;
    }

    public async Task<AffiliateLoginResponseDTO> LoginAsync(LoginRequestDTO loginRequest)
    {
        if (string.IsNullOrWhiteSpace(loginRequest.Email) || string.IsNullOrWhiteSpace(loginRequest.Password))
            throw new ArgumentException("Email e senha são obrigatórios.");

        var affiliate = await _affiliateRepository.GetByEmailAsync(loginRequest.Email);
        if (affiliate == null || !affiliate.IsActive || affiliate.DeletedAt != null)
            throw new UnauthorizedAccessException("Usuário ou senha inválidos.");

        if (!PasswordHelper.VerifyPassword(loginRequest.Password, affiliate.HashPassword))
            throw new UnauthorizedAccessException("Usuário ou senha inválidos.");

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
    
    public async Task<AffiliateLoginResponseDTO> LogoutAsync(string token)
    {
        _logger.LogInformation("[Logout] Token recebido para blacklist: {Token}", token);
        token = token.Trim('"');
        var handler = new JwtSecurityTokenHandler();
        JwtSecurityToken jwtToken;
        try
        {
            jwtToken = handler.ReadJwtToken(token);
        }
        catch
        {
            _logger.LogWarning("[Logout] Tentativa de logout com token inválido: {Token}", token);
            return await Task.FromResult(new AffiliateLoginResponseDTO());
        }
        var expires = jwtToken.ValidTo;
        _logger.LogInformation("[Logout] Token expira em: {Expires}", expires);
        if (expires > DateTime.UtcNow)
        {
            await _tokenBlacklistService.AddToBlacklistAsync(token, expires);
            _logger.LogInformation("[Logout] Token adicionado à blacklist: {Token}", token);
        }
        else
        {
            _logger.LogWarning("[Logout] Token já expirado, não adicionado à blacklist: {Token}", token);
        }
        return await Task.FromResult(new AffiliateLoginResponseDTO());
    }
}