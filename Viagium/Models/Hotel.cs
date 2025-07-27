using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Viagium.Models;

public class Hotel
{
    [Key]
    [Display(Name = "ID do Hotel")]
    public int HotelId { get; set; }
    
    [Required(ErrorMessage = "O nome do hotel é obrigatório.")]
    [Display(Name = "Nome Fantasia do Hotel")]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "A descrição do hotel é obrigatória.")]
    [Display(Name = "Descrição do Hotel")]
    public string Description { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "A URL da imagem do hotel é obrigatória.")]
    [Display(Name = "URL da Imagem do Hotel")]
    public string ImageUrl { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "O telefone de contato do hotel é obrigatório.")]
    [Display(Name = "Telefone de Contato do Hotel")]
    public string ContactNumber { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "O tipo de hospedagem do hotel é obrigatório.")]
    [Display(Name = "Tipo de Hospedagem")]
    public string TypeHosting { get; set; } = string.Empty;
    
    [Required]
    [Display(Name = "Hotel Ativo")]
    public bool IsActive { get; set; } = true;
    
    [Required (ErrorMessage = "O CNPJ do hotel é obrigatório.")]
    [Display(Name = "Cnpj do Hotel")]
    public string Cnpj { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "O Cadastur do hotel é obrigatório.")]
    [Display(Name = "Cadastur do Hotel")]
    public string Cadastur { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "A data de expiração do Cadastur é obrigatória.")]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
    [Display(Name = "Data de Expiração do Cadastur")]
    public DateTime CadasturExpiration { get; set; }
    
    [Display(Name = "Cadastur Válido")]
    public bool CadasturValid => CadasturExpiration > DateTime.Now;
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }

    // Relacionamento com Affiliate
    public int AffiliateId { get; set; }
    public Affiliate? Affiliate { get; set; }
    [Display(Name = "Id do Endereço")]
    [ForeignKey("Address")]
    public int AddressId { get; set; }
    public Address Address { get; set; } = new Address();
    
    public ICollection<HotelAmentity> HotelTypeAmentity { get; set; } = new List<HotelAmentity>();

}
