using Viagium.Models;

namespace Viagium.Repository
{
    public interface ITravelPackageRepository
    {
        Task AddAsync(TravelPackage travel);
    }
}
