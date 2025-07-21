using System.ComponentModel.DataAnnotations;
using Viagium.Models;

namespace Viagium.EntitiesDTO;

// DTO para User
public class UserDTO
{

    [Required]
    [MaxLength(100)]
    [Display(Name = "Nome")]
    public string FirstName { get; set; } = string.Empty;
    [Required]
    [MaxLength(100)]
    [Display(Name = "Sobrenome")]
    public string LastName { get; set; } = string.Empty;
    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;
    [Required]
    public string HashPassword { get; set; } = string.Empty;
    [Required]
    [Display(Name = "Telefone")]
    public string Phone { get; set; } = string.Empty;
    [Required]
    [MaxLength(12)]
    [Display(Name = "Documento de Identificação")]
    public string DocumentNumber { get; set; } = string.Empty;
    [Required]
    [Display(Name = "Data de Nascimento")]
    [DataType(DataType.Date)]
    public DateTime BirthDate { get; set; }
    [Required]
    [Display(Name = "Função")]
    public Role Role { get; set; }
    [Display(Name = "Status")]
    public bool IsActive { get; set; } = true;
   
}
