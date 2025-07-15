using System.ComponentModel.DataAnnotations;

namespace Viagium.Models;

public class Traveler
{
    [Key]
    [Display(Name = "ID do Viajante")]
    public int TravelersId { get; set; }
    
    [Display(Name = "ID da Reserva")]
    public int ReservationId { get; set; } 
    
    
    [Required]
    [MaxLength(100)]
    [Display(Name = "Primeiro Nome")]
    public string FistName { get; set; }
    
    [Required]
    [MaxLength(100)]
    [Display(Name = "Sobrenome")]
    public string LastName { get; set; }
    
    [Required]
    [MaxLength(12)]
    [Display(Name = "Número do Documento")]
    public string DocumentNumber { get; set; }
    
    [Required]
    [Display(Name ="Data de Nascimento")]
    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; } //Verificar se vai ser DateTime ou DateOnly
}