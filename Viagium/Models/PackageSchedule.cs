using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Viagium.Models;

public class PackageSchedule
{
    [Key] public int PackageScheduleId { get; set; }

    [Required]
    [ForeignKey("TravelPackage")]
    [Display(Name = "Pacote de Viagem")]
    public int TravelPackageId { get; set; }

    public TravelPackage? TravelPackage { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Data de Início")]
    public DateTime StartDate { get; set; }

    [NotMapped]
    [Display(Name = "Data de Término")]
    public DateTime EndDate => StartDate.AddDays(TravelPackage?.Duration - 1 ?? 0);
    

    [Display(Name = "É uma data fixa?")] public bool IsFixed { get; set; } = false;

    [Display(Name = "Está disponível?")] public bool IsAvailable { get; set; } = true;

    public virtual ICollection<Reservation>? Reservations { get; set; }
}

