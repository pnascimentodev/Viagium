using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Viagium.Data;
using Viagium.EntitiesDTO;
using Viagium.Models;
using Viagium.Repository.Interface;

namespace Viagium.Repository
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly AppDbContext _context;


        public ReviewRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddReviewAsync(Review review)
        {
            await _context.Reviews.AddAsync(review);
        }

        public async Task<Review?> GetReviewByIdAsync(int id)
        {
            return await _context.Reviews.FindAsync(id);
        }

        public async Task<List<Review>> GetReviewsByReservationIdAsync(int reservationId)
        {
            //Retorna as reviews associadas a uma reserva específica
            return await _context.Reviews
                .Include(r => r.Reservation)
                .Where(r => r.ReservationId == reservationId)
                .ToListAsync();
        }

        public Task<bool> ReviewExistsAsync(int reservationId)
        {            
            //Verifica se existe uma review associada a uma reserva específica
            return _context.Reviews.AnyAsync(r => r.ReservationId == reservationId);
        }
    }
}
