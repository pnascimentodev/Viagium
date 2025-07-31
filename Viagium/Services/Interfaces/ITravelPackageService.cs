using Viagium.EntitiesDTO;
using Viagium.EntitiesDTO.TravelPackageDTO;
using Viagium.Models;

namespace Viagium.Services
{
    public interface ITravelPackage 
    {
       Task<ResponseTravelPackageDTO> AddAsync(CreateTravelPackageDTO createTravelPackageDTO);

    }
}
