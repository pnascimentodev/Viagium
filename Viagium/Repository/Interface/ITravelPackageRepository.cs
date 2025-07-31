using Viagium.EntitiesDTO;
using Viagium.EntitiesDTO.TravelPackageDTO;
using Viagium.Models;

namespace Viagium.Repository
{
    public interface ITravelPackageRepository
    {
        Task<ResponseTravelPackageDTO> AddAsync(CreateTravelPackageDTO createTravelPackageDTO);
        
        Task<ResponseTravelPackageDTO?> UpdateAsync(ResponseTravelPackageDTO responseTravelPackageDTO);
        
        Task<CreateTravelPackageDTO?> AssociateActiveHotelsByCityAndCountry(
            int travelPackageId, string city, string country);

        Task<List<ResponseTravelPackageDTO>> ListAllAsync();
        
        Task<ResponseTravelPackageDTO?> GetByIdAsync(int id);
        
        Task<ResponseTravelPackageDTO?> GetByNameAsync(string name);
        
        Task<ResponseTravelPackageDTO?> GetByCityAndCountryAsync(string city, string country);
        
        Task<List<ResponseTravelPackageDTO>> DesactivateAsync(int id);
        
        Task<List<ResponseTravelPackageDTO>> ActivateAsync(int id);
        
        Task<List<ResponseTravelPackageDTO>> CreateDiscountAsync(
            int travelPackageId, decimal discountPercentage, DateTime startDate, DateTime endDate);
        
        Task<List<ResponseTravelPackageDTO>> DesactivateDiscountAsync(int travelPackageId);
        
        
    }

}
