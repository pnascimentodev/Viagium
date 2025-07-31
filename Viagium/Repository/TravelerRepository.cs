using Microsoft.EntityFrameworkCore;
using Viagium.Data;
using Viagium.Models;
using Viagium.Repository.Interface;

namespace Viagium.Repository
{
    public class TravelerRepository : ITravelerRepository
    {
        private readonly AppDbContext _context;
        public TravelerRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Traveler traveler)
        {
            await _context.Travelers.AddAsync(traveler);
        }

        public async Task<IEnumerable<Traveler>> GetAllAsync()
        {
            return await _context.Travelers.ToListAsync();
        }

        public async Task<Traveler?> GetByIdAsync(int id)
        {
            return await _context.Travelers.FindAsync(id);
        }

        public async Task<IEnumerable<Traveler>> GetByReservationIdAsync(int reservationId)
        {
            return await _context.Travelers.Where(t => t.ReservationId == reservationId).ToListAsync();
        }
    }
}
