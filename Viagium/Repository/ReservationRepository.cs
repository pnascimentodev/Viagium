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
            _context.Reservations.Update(reservation);
            await _context.SaveChangesAsync();
        }
        public async Task<Reservation> DeactivateAsync(int id)
        {
            var existing = await _context.Reservations.FindAsync(id);
            if (existing == null)
                throw new KeyNotFoundException("Reserva não encontrada para desativação.");
            existing.IsActive = false;
            _context.Reservations.Update(existing);
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<IEnumerable<Reservation>> GetByUserIdAsync(int userId)
        {
            return await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Hotel)
                .Include(r => r.RoomType)
                .Include(r => r.TravelPackage)
                .Include(r => r.ReservationRooms)
                .Include(r => r.Travelers)
                .Where(r => r.UserId == userId)
                .ToListAsync();
        }
    }
}
