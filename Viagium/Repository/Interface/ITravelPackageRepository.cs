using Viagium.EntitiesDTO;
using Viagium.EntitiesDTO.TravelPackageDTO;
using Viagium.Models;

namespace Viagium.Repository
{
    public interface ITravelPackageRepository
    {
        Task<ResponseTravelPackageDTO> AddAsync(CreateTravelPackageDTO createTravelPackageDTO);

        Task<CreateTravelPackageDTO?> AssociateActiveHotelsByCityAndCountry(
            int travelPackageId, string city, string country);
    }

}
