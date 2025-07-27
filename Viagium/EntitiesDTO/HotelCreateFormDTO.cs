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
    public string Cadastur { get; set; } = string.Empty;
    [Required]
    [DataType(DataType.Date)]
    public DateTime CadasturExpiration { get; set; }
    [Required]
    public int AffiliateId { get; set; }
    [Required]
    public int AddressId { get; set; }
    public IFormFile Image { get; set; }
    public string? ImageUrl { get; set; }
}
