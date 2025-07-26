namespace Viagium.Models;

public class HotelTypeAmentity
{
    public int HotelId { get; set; }
    public Hotel Hotel { get; set; } = null!;
    
    public int AmenityId { get; set; }
    public Amenity Amenity { get; set; } = null!;
}