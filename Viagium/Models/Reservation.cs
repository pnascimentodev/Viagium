using System.ComponentModel.DataAnnotations;
 
namespace Viagium.Models;
 
public class Reservation
{
    [Key]
    public int ReservationId { get; set; }
 
    [Required]
    public int UserId { get; set; }
 
    [Required]
    public int TravelPackageId { get; set; }  
 
    [Required]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }
 
    [Required]
    public decimal TotalPrice { get; set; }
 
    public string Status { get; set; } = "Pending";
 
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}