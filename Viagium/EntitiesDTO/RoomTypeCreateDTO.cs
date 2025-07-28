using System;
using System.ComponentModel.DataAnnotations;

namespace Viagium.EntitiesDTO;

public class RoomTypeCreateDTO
{
    [Required]
    public int HotelId { get; set; }
    [Required]
    public string Name { get; set; } = string.Empty;
    [Required]
    public string Description { get; set; } = string.Empty;
    [Required]
    public string ImageUrl { get; set; } = string.Empty;
    [Required]
    public decimal PricePerNight { get; set; }
    [Required]
    public int MaxOccupancy { get; set; }
    [Required]
    public int NumberOfRoomsAvailable { get; set; }
    public List<int> Amenities { get; set; } = new();
}
