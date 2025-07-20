// DTO para TravelPackage

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Viagium.Models;

namespace Viagium.EntitiesDTO;

public class CreateTravelPackageDTO
{
    public int HotelId { get; set; }
    public HotelDTO? Hotel { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Título")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    [Display(Name = "Descrição")]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Display(Name = "Origem")]
    public string Origin { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Display(Name = "Destino")]
    public string Destination { get; set; } = string.Empty;

    [Required]
    [Display(Name = "URL da Imagem")]
    public string ImageUrl { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Duração")]
    public int Duration { get; set; }

    [Required]
    [Display(Name = "Máximo de Pessoas")]
    public int MaxPeople { get; set; }

    [Required]
    [Display(Name = "Tipo de Veículo")]
    public string VehicleType { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Currency)]
    [Display(Name = "Preço")]
    public decimal Price { get; set; }

    public int CreatedBy { get; set; }
    public UserDTO? User { get; set; }
    
    public DateTime? CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public bool IsActive { get; set; } = true;
}
