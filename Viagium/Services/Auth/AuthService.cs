using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Viagium.Configurations;
using Viagium.EntitiesDTO.Auth;
using Viagium.Models;
using Viagium.Repository;

namespace Viagium.Services.Auth;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly JwtSettings _jwtSettings;

    public AuthService(IUserRepository userRepository, IOptions<JwtSettings> jwtOptions)
    {
        _userRepository = userRepository;
        _jwtSettings = jwtOptions.Value;
    }

    /// Realiza o login validando a função do usuário.
    public async Task<LoginResponseDTO> LoginWithRoleAsync(LoginRequestDTO loginRequest, Role requiredRole)
    {
        if (string.IsNullOrWhiteSpace(loginRequest.Email) || string.IsNullOrWhiteSpace(loginRequest.Password))
            throw new ArgumentException("Email e senha são obrigatórios.");

        var user = await _userRepository.GetByEmailAsync(loginRequest.Email);
        if (user == null || !user.IsActive || user.DeletedAt != null)
            throw new UnauthorizedAccessException("Usuário ou senha inválidos.");

        if (!PasswordHelper.VerifyPassword(loginRequest.Password, user.HashPassword))
            throw new UnauthorizedAccessException("Usuário ou senha inválidos.");

        if (user.Role != requiredRole)
            throw new UnauthorizedAccessException("Acesso não permitido para este tipo de usuário.");

        var token = GenerateJwtToken(user);
        return new LoginResponseDTO
        {
            Id = user.UserId.ToString(),
            Role = user.Role.ToString(),
            Token = token
        };
    }

    /// Gera um token JWT para o usuário autenticado.
    public string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FirstName + " " + user.LastName),
            new Claim(ClaimTypes.Role, user.Role.ToString())
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