using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Viagium.Models;

namespace Viagium.EntitiesDTO;

public class RoomTypeDTO
{
    public int HotelId { get; set; }
    public HotelDTO? Hotel { get; set; }
    
    [Required]
    [Display(Name = "Nome do Tipo de Quarto")]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [Display(Name = "Descrição")]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [Display(Name = "URL da Imagem")]
    public string ImageUrl { get; set; } = string.Empty;
    
    [Required]
    [Display(Name = "Preço por Noite")]
    public decimal PricePerNight { get; set; }
    
    [Required]
    [Display(Name = "Capacidade Máxima")]
    public int MaxOccupancy { get; set; }
    
    [Required]
    [Display(Name = "Número de Quartos Disponíveis")]
    public int NumberOfRoomsAvailable { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    public List<RoomDTO>? Rooms { get; set; }
}
