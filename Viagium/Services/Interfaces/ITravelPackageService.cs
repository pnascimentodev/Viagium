using Viagium.EntitiesDTO;
using Viagium.Models;

namespace Viagium.Services
{
    public interface ITravelPackage 
    {
        Task<TravelPackage> AddAsync(CreateTravelPackageDTO travelPackageDto);
    }
}
