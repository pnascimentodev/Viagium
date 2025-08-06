using Viagium.EntitiesDTO;
using Viagium.EntitiesDTO.Reservation;
using Viagium.Models;

namespace Viagium.Repository.Interface
{
    public interface IReservationRepository
    {
        Task AddAsync(Reservation reservation);
        Task<IEnumerable<Reservation>> GetAllAsync();
        Task<Reservation?> GetByIdAsync(int id);
        Task<Reservation?> GetByIdWithPaymentAsync(int id); // Novo método
        Task UpdateAsync(Reservation reservation); // Necessário para atualizar status
        Task<Reservation> DeactivateAsync(int id);
        Task<IEnumerable<Reservation>> GetByUserIdAsync(int userId);
        Task<IEnumerable<Reservation>> GetPendingReservationsByUserIdAsync(int userId);
    }
}
