using System.ComponentModel.DataAnnotations.Schema;

namespace Viagium.Models;

public class Amenity
{
    public int AmenityId { get; set; }
    [Column(TypeName = "text")]
    public string Name { get; set; } = null!;
    [Column(TypeName = "text")]
    public string Slug { get; set; } = null!;
    [Column(TypeName = "text")]
    public string IconName { get; set; } = string.Empty;
    [Column(TypeName = "text")]
    public string AmenityScope { get; set; }
    
    public ICollection<RoomTypeAmenity> RoomTypeAmenities { get; set; } = new List<RoomTypeAmenity>();
    public ICollection<HotelAmenity> HotelAmenity { get; set; } = new List<HotelAmenity>();


}