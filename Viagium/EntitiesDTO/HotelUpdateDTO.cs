using System.ComponentModel.DataAnnotations;

namespace Viagium.EntitiesDTO;

public class HotelUpdateDTO
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? ContactNumber { get; set; }
    public string? TypeHosting { get; set; }
    public string? Cnpj { get; set; }
    public string? InscricaoEstadual { get; set; }
    public string? Cadastur { get; set; }
    public DateTime? CadasturExpiration { get; set; }
    
    [Range(1, 5, ErrorMessage = "O número de estrelas deve estar entre 1 e 5.")]
    public int? Star { get; set; }
    
    public string? ImageUrl { get; set; }
    public bool? IsActive { get; set; }
    public AddressDTO? Address { get; set; }
    public List<int>? Amenities { get; set; }
}
