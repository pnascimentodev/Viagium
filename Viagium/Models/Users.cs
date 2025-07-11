using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Security.Cryptography;

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
        [Display(Name = "Sobrenome")]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Documento de Identificação")]
        public string DocumentNumber { get; set; } = string.Empty;
        
        [DataType(DataType.Date)]
        [Display(Name = "Data de Nascimento")]
        public DateTime BirthDate { get; set; }

        [Required]  
        [Display(Name = "Telefone")]
        public string Phone { get; set; }

        [Required]
        [Display(Name = "Senha")]
        public string HashPassword { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; }
        public DateTime? DeletedAt { get; set; }


        public static class PasswordHelper{
             public static string HashPassword(string password){
                using var sha256 = SHA256.Create();
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
             }
        }
    }
}
