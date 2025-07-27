using Viagium.EntitiesDTO;

namespace Viagium.Repository.Interface;

public interface IHotelRepository
{
    Task<HotelWithAddressDTO> AddAsync(HotelCreateFormDTO hotelCreateFormDTO);
    Task<HotelWithAddressDTO?> GetByIdAsync(int id);
    Task<IEnumerable<HotelWithAddressDTO>> GetAllAsync();
    Task UpdateAsync(HotelWithAddressDTO hotelWithAddressDTO);
    Task<bool> DesactivateAsync(int id);
    Task<bool> ActivateAsync(int id);
}
