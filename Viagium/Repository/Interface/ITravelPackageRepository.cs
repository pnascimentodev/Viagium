using Viagium.Models;

namespace Viagium.Repository
{
    public interface ITravelPackageRepository
    {
        Task AddAsync(TravelPackage travel);
        Task<TravelPackage?> GetByIdAsync(int id);
        Task<IEnumerable<TravelPackage>> GetAllAsync();
        Task<TravelPackage> UpdateAsync(TravelPackage travel);
    }
}
