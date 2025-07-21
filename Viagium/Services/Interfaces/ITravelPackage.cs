using Viagium.Models;
using System.Threading.Tasks;

namespace Viagium.Services.Interfaces
{
    public interface ITravelPackage
    {
        Task<TravelPackage?> GetByIdAsync(int id);
        Task<TravelPackage> AddAsync(TravelPackage travelPackage);
    }
}

