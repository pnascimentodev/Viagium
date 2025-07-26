using System.ComponentModel.DataAnnotations;

namespace Viagium.EntitiesDTO;

public class AmenityDTO
{
    public int AmenityId { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string IconName { get; set; } = string.Empty;
}
