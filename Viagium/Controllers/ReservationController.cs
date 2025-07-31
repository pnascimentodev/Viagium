using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Viagium.EntitiesDTO.Reservation;
using Viagium.Services;
using Viagium.Services.Interfaces;

namespace Viagium.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationController : ControllerBase
    {
        private readonly IReservationService _service;
        public ReservationController(IReservationService service)
        {
            _service = service;
        }

        /// <summary>
        /// Cria uma nova reserva.
        /// </summary>
        /// <remarks>Exemplo: POST /api/reservation</remarks>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateReservationDTO createReservationDto)
        {
            try
            {
                ExceptionHandler.ValidateObject(createReservationDto, "reserva");
                var createdReservation = await _service.AddAsync(createReservationDto);
                //return CreatedAtAction(nameof(GetById), new { id = createdReservation.ReservationId }, createdReservation);
                return Ok(createdReservation);

            }
            catch (Exception ex)
            {
                return ExceptionHandler.HandleException(ex);
            }
        }

        /// <summary>
        /// Busca uma reserva pelo ID.
        /// </summary>
        /// <remarks>Exemplo: GET /api/reservation/1</remarks>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var reservation = await _service.GetByIdAsync(id);
            if (reservation == null)
                return NotFound("Reserva não encontrada.");
            return Ok(reservation);
        }

        /// <summary>
        /// Lista todas as reservas cadastradas.
        /// </summary>
        /// <remarks>Exemplo: GET /api/reservation</remarks>
        [HttpGet]
        public async Task<IActionResult> GetAllReservations()
        {
            var reservations = await _service.GetAllAsync();
            return Ok(reservations);
        }


        /// <summary>
        /// Checa e atualiza o status do pagamento da reserva.
        /// </summary>
        /// <remarks>Exemplo: GET /api/reservation/1/check-payment-status</remarks>
        [HttpGet("{id}/check-payment-status")]
        public async Task<IActionResult> CheckAndUpdatePaymentStatus(int id)
        {
            try
            {
                var (paymentStatus, reservationStatus) = await _service.CheckAndUpdatePaymentStatusAsync(id);
                return Ok(new { paymentStatus, reservationStatus });
            }
            catch (Exception ex)
            {
                return ExceptionHandler.HandleException(ex);
            }
        }

        /// <summary>
        /// Desativa uma reserva.
        /// </summary>
        /// <remarks>Exemplo: DELETE /api/reservation/1</remarks>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Deactivate(int id)
        {
            try
            {
                var deactivatedReservation = await _service.DeactivateAsync(id);
                return Ok(deactivatedReservation);
            }
            catch (Exception ex)
            {
                return ExceptionHandler.HandleException(ex);
            }
        }
    }
}
