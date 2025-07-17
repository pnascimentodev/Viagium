using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Viagium.Models;

namespace Viagium.EntitiesDTO;

public class ReservationDTO
{
 
    [Required]
    public User? User { get; set; }
 
    public TravelPackage? TravelPackage { get; set; }
 
    [Required]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }
 
    [Required]
    public decimal TotalPrice { get; set; }
 
    public string Status { get; set; } = "Pending";
 
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    public ICollection<Traveler> Travelers { get; set; }
}
