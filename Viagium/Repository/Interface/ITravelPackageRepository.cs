using Viagium.EntitiesDTO;
using Viagium.Models;

namespace Viagium.Repository
{
    public interface ITravelPackageRepository
    {
        Task AddAsync(TravelPackage travel);
        Task<TravelPackage?> GetByIdAsync(int id);
        Task<IEnumerable<TravelPackage>> GetAllAsync();
        Task<EditTravelPackageDTO> UpdateAsync(int id, EditTravelPackageDTO travelPackage);
        Task<TravelPackage> DesativateAsync(int id);
        Task<TravelPackage> ActivateAsync(int id);
        Task<TravelPackage> ActivePromotionAsync(int id, decimal discountPercentage);
        Task<TravelPackage> DesactivatePromotionAsync(int id);
    }
}
