using Viagium.Models;

namespace Viagium.Services
{
    public interface ITravelPackage 
    {
        Task<TravelPackage> AddAsync(TravelPackage travelPackage);
        Task<TravelPackage?> GetByIdAsync(int id);
        Task<IEnumerable<TravelPackage>> GetAllAsync();
        Task<TravelPackage> UpdateAsync(TravelPackage travelPackage);
        Task<TravelPackage> DesativateAsync(int id);
    }
}
