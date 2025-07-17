using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Viagium.Models;

namespace Viagium.EntitiesDTO;


public class TravelerDTO
{
    
    public Reservation? Reservation { get; set; }
    
    [Required]
    [MaxLength(100)]
    [Display(Name = "Primeiro Nome")]
    public string FirstName { get; set; }
    
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
    public DateTime DateOfBirth { get; set; } 
}
