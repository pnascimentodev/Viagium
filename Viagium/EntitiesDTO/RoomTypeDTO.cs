using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Viagium.Models;

namespace Viagium.EntitiesDTO;


public class RoomTypeDTO
{
    public string Name { get; set; }
    public string Description { get; set; }
    public HotelDTO? Hotel { get; set; }
    public decimal PricePerNight { get; set; }
}
