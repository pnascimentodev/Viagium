using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Viagium.Models;

namespace Viagium.EntitiesDTO;

public class ReservationRoomDTO
{

    
    public ReservationDTO? Reservation { get; set; }
    
    
    public RoomTypeDTO? RoomType { get; set; }
    
    
    public decimal PricePerNight { get; set; }
    
    public int Nights { get; set; }
    
    public decimal TotalPrice { get; set; }
    
}
