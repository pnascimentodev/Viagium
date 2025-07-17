// DTO para TravelPackage

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Viagium.Models;

namespace Viagium.EntitiesDTO;

public class TravelPackageDTO
{

    public Hotel? Hotel { get; set; }

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
    [DataType(DataType.Currency)]
    public decimal Price { get; set; }

    
}
