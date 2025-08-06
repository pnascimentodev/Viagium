using Viagium.EntitiesDTO;

namespace Viagium.Services.Interfaces
{
    public interface IReviewService
    {
        Task<ReviewDTO> AddReviewAsync(ReviewDTO reviewDTO);

        Task<ReviewDTO?> GetReviewByIdAsync(int id);

        Task<List<ReviewDTO>> GetReviewsByReservationIdAsync(int reservationId);

        Task<bool> ReviewExistsAsync(int reservationId);

        //Task<List<ReviewDTO>> GetReviewsByUserIdAsync(int userId);
    }
}
