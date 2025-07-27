namespace Viagium.Models;

public class HotelAmentity
{
    public int HotelId { get; set; }
    public Hotel Hotel { get; set; } = null!;
    
    public int AmenityId { get; set; }
    public Amenity Amenity { get; set; } = null!;
}