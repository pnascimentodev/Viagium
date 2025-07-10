using System.ComponentModel.DataAnnotations;
using Viagium.Models.ENUMS;

namespace Viagium.Models
{
    public class Employees
    {
        [Key]
        [Display(Name = "ID Do Colaborador")]
        public int EmployeeId { get; set; }

        [Required]
        [Display(Name = "Matrícula")]
        public int Registration { get; set; }

        [Display(Name = "Perfil de Usuário")]
        public RoleName RoleName { get; set; } = RoleName.Client; // Default to Client if not specified
    }
}
