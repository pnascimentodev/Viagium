using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Viagium.Models;

namespace Viagium.EntitiesDTO;

public class ReservationDTO
{
    public UserDTO? User { get; set; }
    
    public TravelPackageDTO? TravelPackage { get; set; }
    
    public DateTime StartDate { get; set; }
    
    public decimal TotalPrice { get; set; }
    
    public string Status { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public PaymentDTO? Payment { get; set; }
    
    public List<TravelerDTO>? Travelers { get; set; }
    
    public List<ReservationRoomDTO>? ReservationRooms { get; set; }
}
