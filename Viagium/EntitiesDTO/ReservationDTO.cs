using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Viagium.Models;
using Viagium.EntitiesDTO.User;

namespace Viagium.EntitiesDTO;

public class ReservationDTO
{
    [Required]
    public UserDTO? User { get; set; }

    [Required]
    public TravelPackageDTO? TravelPackage { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; } 
    
    public decimal TotalPrice { get; set; }
    
    public required string Status { get; set; } = "Pending";

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Required]
    public PaymentDTO? Payment { get; set; }
    
    public List<TravelerDTO>? Travelers { get; set; }
    
    public List<ReservationRoomDTO>? ReservationRooms { get; set; }
}
