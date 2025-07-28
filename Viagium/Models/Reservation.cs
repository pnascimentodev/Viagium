using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Viagium.Models;
 
public class Reservation
{
    [Key]
    public int ReservationId { get; set; }
 
    [Required]
    [ForeignKey("User")]
    public int UserId { get; set; }
    public User? User { get; set; }
 
    [Required]
    [ForeignKey("TravelPackage")]
    public int TravelPackageId { get; set; }  
    public TravelPackage? TravelPackage { get; set; }
 
    [Required]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }
 
    [Required]
    public decimal TotalPrice { get; set; }

    public string Status { get; set; } = "Pending"; //reserva p c canc f 
 
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    public required ICollection<Traveler> Travelers { get; set; }
    
    [ForeignKey("Payment")]
    public int PaymentId { get; set; }
    public Payment? Payment { get; set; }
    public required ICollection<ReservationRoom> ReservationRooms { get; set; }

    //desativar a reserva cancelamento
    public bool IsActive { get; internal set; }
}