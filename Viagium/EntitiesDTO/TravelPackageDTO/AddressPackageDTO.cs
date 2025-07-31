using System.ComponentModel.DataAnnotations;

namespace Viagium.EntitiesDTO.TravelPackageDTO;

public class AddressPackageDTO
{

    public int AddressId { get; set; }

    [Required(ErrorMessage = "A cidade é obrigatória.")]
    [Display(Name = "Cidade")]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "O país é obrigatório.")]
    [Display(Name = "Pa�s")]
    public string Country { get; set; } = string.Empty;

    [Display(Name = "Data de Criação")] public DateTime? CreatedAt { get; set; }


}