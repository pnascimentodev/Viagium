namespace Viagium.Models;

public class Amenity
{
    public int AmenityId { get; set; }
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    
    public ICollection<RoomTypeAmenity> RoomTypeAmenities { get; set; } = new List<RoomTypeAmenity>();
    public ICollection<HotelTypeAmentity> HotelTypeAmentities { get; set; } = new List<HotelTypeAmentity>();


}