using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Viagium.Models;

public class RoomType
{
    [Key]
    public int RoomTypeId { get; set; }
    
    [Required]
    [ForeignKey("Hotel")]
    public int HotelId { get; set; }
    public Hotel? Hotel { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public string ImageUrl { get; set; } = string.Empty;
    
    public decimal PricePerNight { get; set; }
    
    public int MaxOccupancy { get; set; }
    
    public int NumberOfRoomsAvailable { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    public bool IsActive { get; set; } = true;
    
    public DateTime? DeletedAt { get; set; }
    
    public ICollection<Room> Rooms { get; set; } = new List<Room>();
    public ICollection<RoomTypeAmenity> RoomTypeAmenities { get; set; } = new List<RoomTypeAmenity>();

}