using System.ComponentModel.DataAnnotations;

namespace Viagium.EntitiesDTO;
public class HotelDTO
{
    public string Name { get; set; }
    public string Location { get; set; }
    public string Description { get; set; }
    public string ImageUrl { get; set; }
    public string ContactNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public AffiliateDTO? Affiliate { get; set; }
}
