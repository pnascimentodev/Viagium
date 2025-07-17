using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Viagium.Models;

public class ReservationRoom
{
    [Key]
    public int ReservationRoomId { get; set; }
    
    [Required]
    [ForeignKey("Reservation")]
    public int ReservationId { get; set; }
    
    public Reservation? Reservation { get; set; }
    
    [Required]
    [ForeignKey("RoomType")]
    public int RoomTypeId { get; set; }
    
    public RoomType? RoomType { get; set; }
    
    public int NumberOfRooms { get; set; }
    
    public decimal PricePerNight { get; set; }
    
    public int Nights { get; set; }
    
    public decimal TotalPrice { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}