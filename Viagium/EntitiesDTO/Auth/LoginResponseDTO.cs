namespace Viagium.EntitiesDTO.Auth;

public class LoginResponseDTO
{
    public string UserId { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}