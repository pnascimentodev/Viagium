using System.ComponentModel.DataAnnotations;

namespace Viagium.Models
{
    public enum Role
    {
        [Display(Name = "Cliente")]
        Client = 1,
        [Display(Name = "Administrador")]
        Admin = 2,
        [Display(Name = "Suporte")]
        Support = 3
    }
}