using System.ComponentModel.DataAnnotations;

namespace Viagium.EntitiesDTO;

public class AddressDTO
{
    [Required(ErrorMessage = "O nome da rua � obrigat�rio.")]
    [Display(Name = "Nome da Rua")]
    public string StreetName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "O n�mero do endere�o � obrigat�rio.")]
    [Display(Name = "N�mero")]
    public int AddressNumber { get; set; }
    
    [Required(ErrorMessage = "O bairro � obrigat�rio.")]
    [Display(Name = "Bairro")]
    public string Neighborhood { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "A cidade � obrigat�ria.")]
    [Display(Name = "Cidade")]
    public string City { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "O estado � obrigat�rio.")]
    [Display(Name = "Estado")]
    public string State { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "O CEP � obrigat�rio.")]
    [Display(Name = "CEP")]
    [RegularExpression(@"^\d{5}-\d{3}$", ErrorMessage = "O CEP deve estar no formato 00000-000.")]
    public string ZipCode { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "O pa�s � obrigat�rio.")]
    [Display(Name = "Pa�s")]
    public string Country { get; set; } = string.Empty;
    [Display(Name = "Data de Criação")]
    public DateTime? CreatedAt { get; set; }
    
    [Display(Name = "Id do Endereço")]
    public int? AddressId { get; set; }
}