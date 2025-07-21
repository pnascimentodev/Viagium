using Viagium.Models;

namespace Viagium.Services.Interfaces
{
    public interface IAddressService
    {
        Task<Address> AddAsync(Address address);
        Task<Address?> GetByIdAsync(int id);
        Task<IEnumerable<Address>> GetAllAsync();
        Task<Address> UpdateAsync(Address address);
    }
}
