using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Viagium.Models;

namespace Viagium.EntitiesDTO;

public class RoomTypeDTO
{
    public int RoomTypeId { get; set; }
    public int HotelId { get; set; }
    
    [Required]
    [Display(Name = "Nome do Tipo de Quarto")]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [Display(Name = "Descrição")]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [Display(Name = "URL da Imagem")]
    public string? ImageUrl { get; set; } = string.Empty;
    
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
    
    public bool IsActive { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    public List<RoomDTO>? Rooms { get; set; }
    public List<AmenityDTO>? Amenities { get; set; } = new();
}
