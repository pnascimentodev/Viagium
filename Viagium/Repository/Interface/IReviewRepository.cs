using Viagium.Models;

namespace Viagium.Repository.Interface
{
    public interface IReviewRepository
    {
        
        Task AddReviewAsync(Review review);
        
        Task<Review?> GetReviewByIdAsync(int id);
        
        Task<List<Review>> GetReviewsByReservationIdAsync(int reservationId);

        Task<bool> ReviewExistsAsync(int reservationId);



        //review por user?
        //Task<List<ReviewDTO>> GetReviewsByUserIdAsync(int userId);
    }
}
