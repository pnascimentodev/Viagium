using Viagium.Models;

namespace Viagium.Repository.Interface
{
    public interface IAddressRepository
    {
        Task AddAsync(Address address);
        Task<Address?> GetByIdAsync(int id);
        Task<IEnumerable<Address>> GetAllAsync();
        Task UpdateAsync(Address address);
    }
}
