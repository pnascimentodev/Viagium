using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace Viagium.EntitiesDTO;

public class HotelCreateFormDTO
{
    [Required]
    public string Name { get; set; } = string.Empty;
    [Required]
    public string Description { get; set; } = string.Empty;
    [Required]
    public string ContactNumber { get; set; } = string.Empty;
    [Required]
    public string TypeHosting { get; set; } = string.Empty;
    [Required]
    public string Cnpj { get; set; } = string.Empty;
    [Required]
    public string InscricaoEstadual { get; set; } = string.Empty;
    [Required]
    public string Cadastur { get; set; } = string.Empty;
    [Required]
    [DataType(DataType.Date)]
    public DateTime CadasturExpiration { get; set; }
    [Required]
    [Range(1, 5, ErrorMessage = "O número de estrelas deve estar entre 1 e 5.")]
    [Display(Name = "Número de Estrelas")]
    public int Star { get; set; } = 1;
    [Required]
    public int AffiliateId { get; set; }
    [Required]
    public AddressDTO Address { get; set; } = new AddressDTO();
    public IFormFile Image { get; set; }
    public string? ImageUrl { get; set; }
    public List<int> Amenities { get; set; } = new();
}
