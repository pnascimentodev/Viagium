using Viagium.Models;

namespace Viagium.Services
{
    public interface ITravelPackage 
    {
        Task<TravelPackage> AddAsync(TravelPackage travelPackage);
        Task<TravelPackage?> GetByIdAsync(int id);
    }
}
