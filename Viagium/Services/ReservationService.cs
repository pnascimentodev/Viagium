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


        public async Task<ResponseReservationDTO?> AddAsync(CreateReservationDTO createReservationDto)
        {
            return await ExceptionHandler.ExecuteWithHandling(async () =>
            {
                // Mapeia o DTO para a entidade Reservation
                var reservation = _mapper.Map<Reservation>(createReservationDto);
                Validator.ValidateObject(reservation, new ValidationContext(reservation), true);
                ValidateCustomRules(reservation);

                // Associa os viajantes à reserva criada
                if (createReservationDto.Travelers != null && createReservationDto.Travelers.Any())
                {
                    foreach (var travelerDto in createReservationDto.Travelers)
                    {
                        var traveler = _mapper.Map<Traveler>(travelerDto);
                        traveler.ReservationId = reservation.ReservationId;
                        await _unitOfWork.TravelerRepository.AddAsync(traveler);
                    }
                    await _unitOfWork.SaveAsync();
                }
                // 1. Criar a Reserva primeiro
                await _unitOfWork.ReservationRepository.AddAsync(reservation);
                await _unitOfWork.SaveAsync();


                return _mapper.Map<ResponseReservationDTO>(reservation);

            }, "criação de reserva");
        }

        private void ValidateCustomRules(CreateReservationDTO reservation)
        {
            var errors = new List<string>();


            if (errors.Any())
                throw new ArgumentException(string.Join("\n", errors));
        }

        public async Task<IEnumerable<ResponseReservationDTO>> GetAllAsync()
        {
            return await ExceptionHandler.ExecuteWithHandling(async () =>
            {
                var reservations = await _unitOfWork.ReservationRepository.GetAllAsync();
                if (reservations.Any() == false)
                    throw new KeyNotFoundException("Nenhuma reserva registrada.");
                return _mapper.Map<IEnumerable<ResponseReservationDTO>>(reservations);
            }, "busca todas as reservas");
        }

        public async Task<ResponseReservationDTO?> GetByIdAsync(int id)
        {
            return await ExceptionHandler.ExecuteWithHandling(async () =>
            {
                var reservation = await _unitOfWork.ReservationRepository.GetByIdAsync(id);
                if (reservation == null)
                    throw new KeyNotFoundException("Reserva por id não encontrado.");

                var dto = _mapper.Map<ResponseReservationDTO>(reservation);
                
                // Busca o hotel através do RoomType da primeira ReservationRoom
                if (reservation.ReservationRooms != null && reservation.ReservationRooms.Any())
                {
                    var roomType = reservation.ReservationRooms.First().RoomType;
                    if (roomType != null)
                    {
                        var hotel = await _unitOfWork.HotelRepository.GetByIdAsync(roomType.HotelId);
                        dto.Hotel = _mapper.Map<HotelDTO>(hotel);
                    }
                }
                
                return dto;
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
