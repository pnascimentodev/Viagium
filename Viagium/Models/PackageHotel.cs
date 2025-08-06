namespace Viagium.Models;

public class PackageHotel
{
    public int TravelPackageId { get; set; }
    public TravelPackage TravelPackage { get; set; }
    public int HotelId { get; set; }
    public Hotel Hotel { get; set; }
}