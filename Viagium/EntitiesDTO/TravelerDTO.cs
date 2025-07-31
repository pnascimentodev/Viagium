using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Viagium.Models;

namespace Viagium.EntitiesDTO;


public class TravelerDTO
{

    //public ReservationDTO? Reservation { get; set; }

    [Required]
    [MaxLength(100)]
    [Display(Name = "Primeiro Nome")]
    public required string FirstName { get; set; }

    [Required]
    [MaxLength(100)]
    [Display(Name = "Sobrenome")]
    public required string LastName { get; set; }

    [Required]
    [MaxLength(12)]
    [Display(Name = "Número do Documento")]
    public required string DocumentNumber { get; set; }

    [Required]
    [Display(Name = "Data de Nascimento")]
    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; }
}
