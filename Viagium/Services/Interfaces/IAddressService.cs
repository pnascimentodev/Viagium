using Viagium.EntitiesDTO;

namespace Viagium.Services.Interfaces
{
    public interface IAddressService
    {
        Task<AddressDTO> AddAsync(AddressDTO addressDto);
        Task<AddressDTO?> GetByIdAsync(int id);
        Task<IEnumerable<AddressDTO>> GetAllAsync();
        Task<AddressDTO> UpdateAsync(int id, AddressDTO addressDto);
    }
}
