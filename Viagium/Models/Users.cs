using System.ComponentModel.DataAnnotations;

namespace Viagium.Models
{
    public class Users
    {
        [Key]
        [Display(Name = "Id do Usuário")]
        public int UserId { get; set; }
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
        [Display(Name = "Documento de Identificação")]
        public string DocumentNumber { get; set; } = string.Empty;
        [Required]
        [Display(Name = "Data de Nascimento")]
        [DataType(DataType.Date)]
        public DateTime DataNascimento { get; set; }
        [Required]
        [Display(Name = "Função")]
        public Role Role { get; set; }
        [Display(Name = "Status")]
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; }
        public DateTime DeletedAt { get; set; }
    }
}
