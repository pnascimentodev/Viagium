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


        [HttpGet("{id}")]
        public async Task<IActionResult> GetReviewByIdAsync(int id)
        {
            var review = await _review.GetReviewByIdAsync(id);            
            return Ok(review);
        }


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


        [HttpGet("exists/{reservationId}")]
        public async Task<IActionResult> ReviewExistsAsync(int reservationId)
        {
            var exists = await _review.ReviewExistsAsync(reservationId);
            return Ok(exists);
        }


    }
}
