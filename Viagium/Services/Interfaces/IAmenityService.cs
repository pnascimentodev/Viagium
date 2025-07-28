using Viagium.EntitiesDTO;

namespace Viagium.Services.Interfaces;

public interface IAmenityService
{
    Task<IEnumerable<AmenityDTO>> GetAllAsync();
    Task<IEnumerable<AmenityDTO>> GetHotelAmenitiesAsync();
    Task<IEnumerable<AmenityDTO>> GetRoomTypeAmenitiesAsync();
}