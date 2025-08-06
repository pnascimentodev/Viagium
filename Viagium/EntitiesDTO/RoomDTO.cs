using System.ComponentModel.DataAnnotations;

namespace Viagium.EntitiesDTO;

public class RoomDTO
{
    public int RoomTypeId { get; set; }
  
    [Required]
    [Display(Name = "N�mero do Quarto")]
    public string RoomNumber { get; set; } = string.Empty;
    
    [Display(Name = "Dispon�vel")]
    public bool IsAvailable { get; set; } = true;
}