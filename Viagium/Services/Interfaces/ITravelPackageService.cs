using Viagium.EntitiesDTO;
using Viagium.Models;

namespace Viagium.Services
{
    public interface ITravelPackage 
    {
        Task<TravelPackage> AddAsync(TravelPackage travelPackage);
        Task<TravelPackage?> GetByIdAsync(int id);
        Task<IEnumerable<TravelPackage>> GetAllAsync();
        Task<EditTravelPackageDTO> UpdateAsync(int id, EditTravelPackageDTO travelPackage);
        Task<TravelPackage> DesativateAsync(int id);
        Task<TravelPackage> ActivateAsync(int id);
        Task<TravelPackage> ActivePromotionAsync(int id, decimal discountPercentage);
    }
}
