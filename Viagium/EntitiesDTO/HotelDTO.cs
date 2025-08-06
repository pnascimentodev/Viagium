using System.ComponentModel.DataAnnotations;

namespace Viagium.EntitiesDTO;

public class HotelDTO
{
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
    
    [Required (ErrorMessage = "O CNPJ do hotel é obrigatório.")]
    [Display(Name = "Cnpj do Hotel")]
    public string Cnpj { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "A Inscrição Estadual do hotel é obrigatória.")]
    [Display(Name = "Inscrição Estadual do Hotel")]
    public string InscricaoEstadual { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "O Cadastur do hotel é obrigatório.")]
    [Display(Name = "Cadastur do Hotel")]
    public string Cadastur { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "A data de expiração do Cadastur é obrigatória.")]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
    [Display(Name = "Data de Expiração do Cadastur")]
    public DateTime CadasturExpiration { get; set; }
    
    [Required]
    [Range(1, 5, ErrorMessage = "O número de estrelas deve estar entre 1 e 5.")]
    [Display(Name = "Número de Estrelas")]
    public int Star { get; set; } = 1;
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }

    // Relacionamento com Affiliate
    public int AffiliateId { get; set; }
    public AffiliateDTO? Affiliate { get; set; }
    
    [Display(Name = "Id do Endereço")]
    public int AddressId { get; set; }
    public AddressDTO? Address { get; set; }
}
