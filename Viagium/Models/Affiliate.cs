using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Viagium.Models;

public class Affiliate
{
    public int AffiliateId { get; set; }
    
    [Required]
    [StringLength(100)]
    [Display(Name = "Nome Fantasia")]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    [Display(Name = "CNPJ")]
    public string Cnpj { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    [Display(Name = "Razão Social")]
    public string CompanyName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    [Display(Name = "E-mail")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [StringLength(15)]
    [Display(Name = "Telefone")]
    public string Phone { get; set; } = string.Empty;
    
    [Required]
    [StringLength(200)]
    [Display(Name = "Endereço")]
    public string Address { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    [Display(Name = "Inscrição Estadual")]
    public string StateRegistration { get; set; } = string.Empty;
    
    [Required]
    public string HashPassword { get; set; } = string.Empty;
    
    [Display(Name = "Data de Criação")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    [Display(Name = "Data de Atualização")]
    public DateTime? UpdatedAt { get; set; }
    
    [Display(Name = "Status")]
    public bool IsActive { get; set; } = true;
    
    [Display(Name = "Data de Exclusão")]
    public DateTime? DeletedAt { get; set; }
    
    // Relacionamento: Um afiliado pode registrar vários hotéis
    public ICollection<Hotel> Hotels { get; set; } = new List<Hotel>();
}