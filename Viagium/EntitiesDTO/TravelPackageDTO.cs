// DTO para TravelPackage

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Viagium.Models;

namespace Viagium.EntitiesDTO;

public class TravelPackageDTO
{
    public HotelDTO? Hotel { get; set; }

    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Destination { get; set; } = string.Empty;

    [Required]
    public string ImageUrl { get; set; } = string.Empty;

    [Required]
    public int Duration { get; set; }

    [Required]
    public int MaxPeople { get; set; }

    [Required]
    public string VehicleType { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Currency)]
    public decimal Price { get; set; }

    public UserDTO? User { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
