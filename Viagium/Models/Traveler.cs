using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Viagium.Models;

public class Traveler
{
    [Key]
    [Display(Name = "ID do Viajante")]
    public int TravelersId { get; set; }
    
    [Display(Name = "ID da Reserva")]
    [ForeignKey("Reservation")]
    public int ReservationId { get; set; } 
    public Reservation? Reservation { get; set; }
    
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
    [Display(Name ="Data de Nascimento")]
    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; } 
}