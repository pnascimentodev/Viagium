using System.ComponentModel.DataAnnotations;
using Viagium.EntitiesDTO;

namespace Viagium.EntitiesDTO.Affiliate;

public class AffiliateCreateDto
{
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
    [StringLength(100)]
    [Display(Name = "Inscrição Estadual")]
    public string StateRegistration { get; set; } = string.Empty;
    
    [Required]
    public string HashPassword { get; set; } = string.Empty;
    
    [Required]
    [Display(Name = "Número do Cadastur")]
    [StringLength(14)]
    public string NumberCadastur { get; set; } = string.Empty;
    
    [Required]
    [Display(Name = "Data de Emissão do Cadastur")]
    [DataType(DataType.Date)]
    public DateTime ExpirationDate { get; set; }

    [Required]
    [Display(Name = "Esta Ativo no Cadastur?")]
    public bool IsActiveCadastur { get; set; } = true;
    
    // Endereço obrigatório
    [Required]
    [Display(Name = "Endereço")]
    public AddressDTO Address { get; set; } = new AddressDTO();
    
}