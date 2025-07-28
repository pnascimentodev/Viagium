namespace Viagium.EntitiesDTO.User
{
    public class UpdatePasswordDto
    {
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}

