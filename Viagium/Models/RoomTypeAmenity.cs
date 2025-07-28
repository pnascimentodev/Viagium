namespace Viagium.Models;

public class RoomTypeAmenity
{
    public int RoomTypeId { get; set; }
    public RoomType RoomType { get; set; } = null!;

    public int AmenityId { get; set; }
    public Amenity Amenity { get; set; } = null!;
}