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



        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateReservationDTO createReservationDto)
        {
            try
            {
                ExceptionHandler.ValidateObject(createReservationDto, "reserva");
                var createdReservation = await _service.AddAsync(createReservationDto);
                //return Ok(createdAddress);
                return CreatedAtAction(nameof(GetById), new { id = createdReservation.ReservationId }, createdReservation);

            }
            catch (Exception ex)
            {
                return ExceptionHandler.HandleException(ex);
            }
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var reservation = await _service.GetByIdAsync(id);
            if (reservation == null)
                return NotFound("Reserva não encontrada.");
            return Ok(reservation);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllReservations()
        {
            var reservations = await _service.GetAllAsync();
            return Ok(reservations);
        }


        //Checa e atualiza o status do pagamento caso o pagamento não tenha sido realizado
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
