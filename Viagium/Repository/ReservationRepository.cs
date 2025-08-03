using Microsoft.EntityFrameworkCore;
using Viagium.Data;
using Viagium.Models;
using Viagium.Repository.Interface;

namespace Viagium.Repository
{
    public class ReservationRepository : IReservationRepository
    {
        private readonly AppDbContext _context;
        public ReservationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Reservation reservation)
        {
            await _context.Reservations.AddAsync(reservation);
        }


        public async Task<IEnumerable<Reservation>> GetAllAsync()
        {
            return await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Hotel) // Incluir o Hotel diretamente
                    .ThenInclude(h => h.Address) // Incluir o endereço do hotel
                .Include(r => r.RoomType) // Incluir o RoomType diretamente
                .Include(r => r.TravelPackage)
                    .ThenInclude(tp => tp.OriginAddress)
                .Include(r => r.TravelPackage)
                    .ThenInclude(tp => tp.DestinationAddress)
                .Include(r => r.TravelPackage)
                    .ThenInclude(tp => tp.User)
                .Include(r => r.ReservationRooms)
                    .ThenInclude(rr => rr.RoomType)
                .Include(r => r.Travelers)
                .ToListAsync();
        }

        public async Task<Reservation?> GetByIdAsync(int id)
        {
            return await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Hotel) // Incluir o Hotel diretamente
                    .ThenInclude(h => h.Address) // Incluir o endereço do hotel
                .Include(r => r.RoomType) // Incluir o RoomType diretamente
                .Include(r => r.TravelPackage)
                    .ThenInclude(tp => tp.OriginAddress)
                .Include(r => r.TravelPackage)
                    .ThenInclude(tp => tp.DestinationAddress)
                .Include(r => r.TravelPackage)
                    .ThenInclude(tp => tp.User)
                .Include(r => r.ReservationRooms)
                    .ThenInclude(rr => rr.RoomType)
                .Include(r => r.Travelers)
                .FirstOrDefaultAsync(r => r.ReservationId == id);
        }

        public async Task<Reservation?> GetByIdWithPaymentAsync(int id)
        {
            return await _context.Reservations.Include(r => r.Payment).FirstOrDefaultAsync(r => r.ReservationId == id);
        }

        public async Task UpdateAsync(Reservation reservation)
        {
            // ✅ CORREÇÃO: Apenas atualiza a entidade, não salva diretamente
            _context.Reservations.Update(reservation);
            // Removido: await _context.SaveChangesAsync(); - isso deve ser feito pelo UnitOfWork
        }
        public async Task<Reservation> DeactivateAsync(int id)
        {
            var existing = await _context.Reservations.FindAsync(id);
            if (existing == null)
                throw new KeyNotFoundException("Reserva não encontrada para desativação.");
            existing.IsActive = false;
            _context.Reservations.Update(existing);
            // ✅ CORREÇÃO: Removido SaveChanges - deve ser feito pelo UnitOfWork
            return existing;
        }
    }
}
