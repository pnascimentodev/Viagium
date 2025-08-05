using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Viagium.EntitiesDTO.TravelPackageDTO;

public class PackageScheduleDTO
{
    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Data de Início")]
    public DateTime StartDate { get; set; }

    [Display(Name = "Está disponível?")] 
    public bool IsAvailable { get; set; } = true;
}