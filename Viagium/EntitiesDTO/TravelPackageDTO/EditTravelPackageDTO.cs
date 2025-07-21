using System.ComponentModel.DataAnnotations;
using Viagium.Models;

namespace Viagium.EntitiesDTO;

public class EditTravelPackageDTO
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
    public int OriginAddressId { get; set; }
    public Address? OriginAddress { get; set; }

    [Required]
    public int DestinationAddressId { get; set; }
    public Address? DestinationAddress { get; set; }


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

    [Required]
    [Display(Name = "Criado por")]
    public int CreatedBy { get; set; }

    [Required]
    [Display(Name = "Ativo")]
    public bool IsActive { get; set; } = true;

    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}