using Viagium.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Viagium.Services.Interfaces
{
    public interface ITravelerService
    {
        Task AddAsync(Traveler traveler);
        Task<IEnumerable<Traveler>> GetAllAsync();
        Task<Traveler?> GetByIdAsync(int id);
        Task<IEnumerable<Traveler>> GetByReservationIdAsync(int reservationId);
    }
}
