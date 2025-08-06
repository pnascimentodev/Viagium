using System.ComponentModel.DataAnnotations;

namespace Viagium.EntitiesDTO;

public class RoomDTO
{
    public int RoomTypeId { get; set; }
  
    [Required]
    [Display(Name = "Número do Quarto")]
    public string RoomNumber { get; set; } = string.Empty;
    
    [Display(Name = "Disponível")]
    public bool IsAvailable { get; set; } = true;
}