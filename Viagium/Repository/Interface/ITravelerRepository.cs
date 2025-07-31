using Viagium.Models;

namespace Viagium.Repository.Interface
{
    public interface ITravelerRepository
    {
        Task AddAsync(Traveler traveler);
        Task<IEnumerable<Traveler>> GetAllAsync();
        Task<Traveler?> GetByIdAsync(int id);
        Task<IEnumerable<Traveler>> GetByReservationIdAsync(int reservationId);
    }
}
