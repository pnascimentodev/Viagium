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
            return await _context.Reservations.ToListAsync();
        }

        public async Task<Reservation?> GetByIdAsync(int id)
        {
            return await _context.Reservations.FindAsync(id);
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
    }
}
