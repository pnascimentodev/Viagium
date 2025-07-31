namespace Viagium.EntitiesDTO;

public class HotelWithAddressDTO
{
    public int HotelId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
    public string TypeHosting { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string Cnpj { get; set; } = string.Empty;
    public string Cadastur { get; set; } = string.Empty;
    public DateTime CadasturExpiration { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public AddressDTO Address { get; set; } = new AddressDTO();
    public List<AmenityDTO> Amenities { get; set; } = new();
    public List<RoomTypeDTO> RoomTypes { get; set; } = new();
    public int AffiliateId { get; set; } // Corrigido para ser int, compatível com Hotel
}
