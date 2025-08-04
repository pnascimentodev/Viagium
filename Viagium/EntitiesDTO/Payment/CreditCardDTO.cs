using System.ComponentModel.DataAnnotations;

namespace Viagium.EntitiesDTO.Payment;

public class CreditCardDTO
{
    [Required(ErrorMessage = "Nome do portador é obrigatório")]
    [Display(Name = "Nome do Portador")]
    public string HolderName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Número do cartão é obrigatório")]
    [Display(Name = "Número do Cartão")]
    [StringLength(19, MinimumLength = 13, ErrorMessage = "Número do cartão deve ter entre 13 e 19 dígitos")]
    public string Number { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Mês de expiração é obrigatório")]
    [Display(Name = "Mês de Expiração")]
    [Range(1, 12, ErrorMessage = "Mês deve estar entre 1 e 12")]
    public string ExpiryMonth { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Ano de expiração é obrigatório")]
    [Display(Name = "Ano de Expiração")]
    [StringLength(4, MinimumLength = 4, ErrorMessage = "Ano deve ter 4 dígitos")]
    public string ExpiryYear { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "CCV é obrigatório")]
    [Display(Name = "Código de Segurança")]
    [StringLength(4, MinimumLength = 3, ErrorMessage = "CCV deve ter 3 ou 4 dígitos")]
    public string Ccv { get; set; } = string.Empty;
}
