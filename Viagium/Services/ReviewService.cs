using AutoMapper;
using Viagium.EntitiesDTO;
using Viagium.Models;
using Viagium.Repository.Interface;
using Viagium.Services.Interfaces;

namespace Viagium.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ReviewService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }


        public async Task<ReviewDTO> AddReviewAsync(ReviewDTO reviewDTO)
        {
            var review = _mapper.Map<Review>(reviewDTO);

            if (review == null)
            {
                throw new ArgumentNullException(nameof(reviewDTO), "Review nao pode ser vazia");
            }
            await _unitOfWork.ReviewRepository.AddReviewAsync(review);
            await _unitOfWork.SaveAsync();
            return _mapper.Map<ReviewDTO>(review);
        }



        public async Task<ReviewDTO?> GetReviewByIdAsync(int id)
        {
            var review = await _unitOfWork.ReviewRepository.GetReviewByIdAsync(id);
            if (review == null)
            {
                throw new KeyNotFoundException("Review nao encontrado pelo ID.");
            }
            return _mapper.Map<ReviewDTO>(review);

        }


        public async Task<List<ReviewDTO>> GetReviewsByReservationIdAsync(int reservationId)
        {
            var reviews = await _unitOfWork.ReviewRepository.GetReviewsByReservationIdAsync(reservationId);
            if (reviews == null || !reviews.Any())
            {
                throw new KeyNotFoundException("Nenhuma review encontrada para a reserva especificada.");
            }
            return _mapper.Map<List<ReviewDTO>>(reviews);
        }



        public async Task<bool> ReviewExistsAsync(int reservationId)
        {
            // Verifica se existe uma review associada a uma reserva específica
            //Retorna True se existir, False caso contrario
            return await _unitOfWork.ReviewRepository.ReviewExistsAsync(reservationId);

        }
    }
}
