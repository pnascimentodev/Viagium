using System.Runtime.Intrinsics.X86;
using Viagium.EntitiesDTO;
using Viagium.EntitiesDTO.Reservation;
using Viagium.Models;

namespace Viagium.Services.Interfaces
{
    public interface IReservationService
    {
        Task<Reservation> AddAsync(CreateReservationDTO createReservationDto);
        Task<ReservationDTO?> GetByIdAsync(int id);
        Task<IEnumerable<ReservationDTO>> GetAllAsync();

        // Uma tupla é uma estrutura que pode agrupar múltiplos valores de tipos diferentes em um único retorno.
        Task<(string paymentStatus, string reservationStatus)> CheckAndUpdatePaymentStatusAsync(int reservationId); // Novo método

        Task<Reservation> DeactivateAsync(int id);
    }
}
