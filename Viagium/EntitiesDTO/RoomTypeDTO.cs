using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Viagium.Models;

namespace Viagium.EntitiesDTO;


public class RoomTypeDTO
{
    

    public Hotel? Hotel { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public decimal PricePerNight { get; set; }
    
    public int NumberOfRoomsAvailable { get; set; }
    
}
