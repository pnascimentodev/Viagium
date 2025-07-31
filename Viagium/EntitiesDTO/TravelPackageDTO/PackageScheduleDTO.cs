using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Viagium.Models;

namespace Viagium.EntitiesDTO.TravelPackageDTO;

public class PackageScheduleDTO
{
    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Data de Início")]
    public DateTime StartDate { get; set; }

    [NotMapped]
    [Display(Name = "Data de Término")]
    public DateTime? EndDate
    {
        get
        {
            if (Duration <= 0) return null;
            try
            {
                return StartDate.AddDays(Duration - 1);
            }
            catch (ArgumentOutOfRangeException)
            {
                return null;
            }
        }
    }

    public int Duration { get; set; }

    [Display(Name = "É uma data fixa?")] public bool IsFixed { get; set; } = false;

    [Display(Name = "Está disponível?")] public bool IsAvailable { get; set; } = true;

}