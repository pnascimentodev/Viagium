using System.ComponentModel.DataAnnotations;

namespace Viagium.Models
{
    public enum UserRole
    {
        [Display(Name = "Usuário")]
        User = 1,
        
        [Display(Name = "Administrador")]
        Admin = 2,
        
        [Display(Name = "Moderador")]
        Moderator = 3
    }
}