namespace Viagium.Models;

public class AmentityHotel
{
    public int AmenityId { get; set; }
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string IconName { get; set; } = string.Empty;

    public int HotelId { get; set; }
    public Hotel Hotel { get; set; } = null!;
    public Amenity Amenity { get; set; } = null!;
}