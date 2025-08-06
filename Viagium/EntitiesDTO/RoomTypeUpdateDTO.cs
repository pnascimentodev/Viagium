using System;
using System.ComponentModel.DataAnnotations;

namespace Viagium.EntitiesDTO;

public class RoomTypeUpdateDTO
{
    [Required]
    public int RoomTypeId { get; set; }
    [Required]
    public int HotelId { get; set; }
    [Required]
    public string Name { get; set; } = string.Empty;
    [Required]
    public string Description { get; set; } = string.Empty;
    [Required]
    public string ImageUrl { get; set; } = string.Empty;
    public IFormFile? ImageFile { get; set; }
    [Required]
    public decimal PricePerNight { get; set; }
    [Required]
    public int MaxOccupancy { get; set; }
    [Required]
    public int NumberOfRoomsAvailable { get; set; }
}
