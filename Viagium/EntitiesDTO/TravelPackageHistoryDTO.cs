using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Viagium.Models;

namespace Viagium.EntitiesDTO;


public class TravelPackageHistoryDTO
{

    public Reservation? Reservation { get; set; }

    [Required]
    [MaxLength(50)]
    [Display(Name = "Título")]
    public string? Title { get; set; }

    [Required]
    [MaxLength(2000)]
    [Display(Name = "Descrição")]
    public string? Description { get; set; }

    [Required]
    [MaxLength(100)]
    [Display(Name = "Destino")]
    public string? Destination { get; set; }

    [Required]
    [Display(Name = "Duração")]
    public int Duration { get; set; }

    [Required]
    [Display(Name = "Preço")]
    public decimal Price { get; set; }
    
}
