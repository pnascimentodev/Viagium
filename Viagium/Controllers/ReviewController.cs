using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Viagium.EntitiesDTO;
using Viagium.Services.Interfaces;

namespace Viagium.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _review;

        public ReviewController(IReviewService review)
        {
            _review = review;
        }


        /// <summary>
        /// Adiciona uma nova avaliação.
        /// </summary>
        /// <remarks>Exemplo: POST /api/review</remarks>
        [HttpPost]
        public async Task<IActionResult> AddReviewAsync([FromBody] ReviewDTO reviewDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdReview = await _review.AddReviewAsync(reviewDTO);

            return Ok(createdReview);
        }


        /// <summary>
        /// Obtém uma avaliação pelo ID.
        /// </summary>
        /// <remarks>Exemplo: GET /api/review/1</remarks>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReviewByIdAsync(int id)
        {
            var review = await _review.GetReviewByIdAsync(id);
            return Ok(review);
        }


        /// <summary>
        /// Obtém avaliações pelo ID da reserva.
        /// </summary>
        /// <remarks>Exemplo: GET /api/review/reservation/1</remarks>
        [HttpGet("reservation/{reservationId}")]
        public async Task<IActionResult> GetReviewsByReservationIdAsync(int reservationId)
        {
            try
            {
                var reviews = await _review.GetReviewsByReservationIdAsync(reservationId);
                return Ok(reviews);
            }
            catch (KeyNotFoundException ex)
            {
                //Captura o erro advindo do service e retorna um NotFound
                return NotFound(ex.Message);
            }
        }


        /// <summary>
        /// Verifica se uma avaliação existe para um determinado ID de reserva.
        /// </summary>
        /// <remarks>Exemplo: GET /api/review/exists/1</remarks>
        [HttpGet("exists/{reservationId}")]
        public async Task<IActionResult> ReviewExistsAsync(int reservationId)
        {
            var exists = await _review.ReviewExistsAsync(reservationId);
            return Ok(exists);
        }
    }
}