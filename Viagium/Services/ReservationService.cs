using AutoMapper;
using System.ComponentModel.DataAnnotations;
using Viagium.EntitiesDTO;
using Viagium.EntitiesDTO.Reservation;
using Viagium.Models;
using Viagium.Repository.Interface;
using Viagium.Services.Interfaces;

namespace Viagium.Services
{
    public class ReservationService : IReservationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ReservationService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }


        public async Task<Reservation> AddAsync(CreateReservationDTO createReservationDto)
        {
            return await ExceptionHandler.ExecuteWithHandling(async () =>
            {
                //Mapeou o que vem de DTO pra model Reservation
                var reservation = _mapper.Map<Reservation>(createReservationDto);
                Validator.ValidateObject(reservation, new ValidationContext(reservation), true);
                // Validações customizadas específicas do negócio
                ValidateCustomRules(reservation);
                await _unitOfWork.ReservationRepository.AddAsync(reservation);
                await _unitOfWork.SaveAsync();
                return _mapper.Map<Reservation>(reservation);
            }, "criação de reserva");
        }

        public async Task<IEnumerable<ReservationDTO>> GetAllAsync()
        {
            return await ExceptionHandler.ExecuteWithHandling(async () =>
            {
                var reservations = await _unitOfWork.ReservationRepository.GetAllAsync();
                if(reservations.Any() == false)
                    throw new KeyNotFoundException("Nenhuma reserva registrada.");
                return _mapper.Map<IEnumerable<ReservationDTO>>(reservations);
            }, "busca todas as reservas");
        }

        public async Task<ReservationDTO?> GetByIdAsync(int id)
        {
            return await ExceptionHandler.ExecuteWithHandling(async () =>
            {
                var reservation = await _unitOfWork.ReservationRepository.GetByIdAsync(id);
                if (reservation == null)
                    throw new KeyNotFoundException("Reserva por id não encontrado.");

                return _mapper.Map<ReservationDTO>(reservation);
            }, "busca de reserva pelo id");
        }

        public async Task<(string paymentStatus, string reservationStatus)> CheckAndUpdatePaymentStatusAsync(int reservationId)
        {
            var reservation = await _unitOfWork.ReservationRepository.GetByIdWithPaymentAsync(reservationId);
            if (reservation == null)
                throw new KeyNotFoundException("Reserva não encontrada.");
            if (reservation.Payment == null)
                throw new KeyNotFoundException("Pagamento não encontrado para esta reserva.");

            var paymentStatus = reservation.Payment.Status.ToLower();
            if (paymentStatus == "approved" && reservation.Status != "completed")
            {
                reservation.Status = "completed";
                await _unitOfWork.ReservationRepository.UpdateAsync(reservation);
                await _unitOfWork.SaveAsync();
            }
            return (paymentStatus, reservation.Status);
        }

        //Caso tenha alguma validacao posterior
        
        
        private void ValidateCustomRules(Reservation reservation)
        {
            var errors = new List<string>();
                      

            if (errors.Any())
                throw new ArgumentException(string.Join("\n", errors));
        }

        public async Task<Reservation> DeactivateAsync(int id)
        {
            return await ExceptionHandler.ExecuteWithHandling(async () =>
            {
                var reservation = await _unitOfWork.ReservationRepository.DeactivateAsync(id);
                return reservation;

            }, "reserva desativada");
        }
    }
}
