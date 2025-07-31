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

                //PaymentId esta null porem quando for criado o pagamento,
                //ele será atualizado para gerar um PaymentId de pagamento???
                //Como estava antes???


                // Validar se o hotel existe antes de criar a reserva
                var hotelExists = await _unitOfWork.HotelRepository.GetByIdAsync(createReservationDto.HotelId);
                if (hotelExists == null)
                    throw new KeyNotFoundException($"Hotel com ID {createReservationDto.HotelId} não encontrado.");

                reservation.IsActive = true; // Define a reserva como ativa
                reservation.HotelId = createReservationDto.HotelId; // Define o HotelId diretamente na reserva
                
                // 1. Criar a Reserva primeiro para obter o ReservationId
                await _unitOfWork.ReservationRepository.AddAsync(reservation);
                await _unitOfWork.SaveAsync();

                // 2. Associar os viajantes à reserva criada (agora que temos o ReservationId)
                if (createReservationDto.Travelers != null && createReservationDto.Travelers.Any())
                {
                    foreach (var travelerDto in createReservationDto.Travelers)
                    {
                        var traveler = _mapper.Map<Traveler>(travelerDto);
                        traveler.ReservationId = reservation.ReservationId; // Agora o ReservationId existe
                        
                        Validator.ValidateObject(traveler, new ValidationContext(traveler), true);
                        await _unitOfWork.TravelerRepository.AddAsync(traveler);
                    }                    
                }

                // 3. Buscar a reserva completa com todos os dados para retorno
                var completeReservation = await _unitOfWork.ReservationRepository.GetByIdAsync(reservation.ReservationId);
                var dto = _mapper.Map<ResponseReservationDTO>(completeReservation);
                
                // Buscar os travelers manualmente para evitar duplicação
                var travelers = await _unitOfWork.TravelerRepository.GetByReservationIdAsync(reservation.ReservationId);
                dto.Travelers = _mapper.Map<List<TravelerDTO>>(travelers);
                dto.IsActive = reservation.IsActive; // Incluir o status ativo da reserva
                
                // O Hotel agora vem automaticamente através do relacionamento direto
                // Mas vamos garantir que os dados estejam completos como fallback
                if (dto.Hotel == null && hotelExists != null)
                {
                    dto.Hotel = _mapper.Map<HotelDTO>(hotelExists);
                }
                
                return dto;

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
                
                var dtos = new List<ResponseReservationDTO>();

                // Buscar os travelers manualmente para cada reserva para evitar duplicação
                foreach (var reservation in reservations)
                {
                    var dto = _mapper.Map<ResponseReservationDTO>(reservation);
                    var travelers = await _unitOfWork.TravelerRepository.GetByReservationIdAsync(reservation.ReservationId);
                    dto.Travelers = _mapper.Map<List<TravelerDTO>>(travelers);
                    dto.IsActive = reservation.IsActive; // Incluir o status ativo da reserva
                    
                    // O Hotel agora vem automaticamente através do relacionamento direto
                    // Se não vier pelo AutoMapper, buscar manualmente como fallback
                    if (dto.Hotel == null && reservation.HotelId.HasValue)
                    {
                        var hotelData = await _unitOfWork.HotelRepository.GetByIdAsync(reservation.HotelId.Value);
                        if (hotelData != null)
                        {
                            dto.Hotel = _mapper.Map<HotelDTO>(hotelData);
                        }
                    }
                    
                    dtos.Add(dto);
                }

                return dtos;
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

                // Buscar os travelers manualmente para evitar duplicação
                var travelers = await _unitOfWork.TravelerRepository.GetByReservationIdAsync(id);
                dto.Travelers = _mapper.Map<List<TravelerDTO>>(travelers);
                dto.IsActive = reservation.IsActive; // Incluir o status ativo da reserva

                // O Hotel agora vem automaticamente através do relacionamento direto
                // Se não vier pelo AutoMapper, buscar manualmente como fallback
                if (dto.Hotel == null && reservation.HotelId.HasValue)
                {
                    var hotelData = await _unitOfWork.HotelRepository.GetByIdAsync(reservation.HotelId.Value);
                    if (hotelData != null)
                    {
                        dto.Hotel = _mapper.Map<HotelDTO>(hotelData);
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
                // Validação do ID
                if (id <= 0)
                    throw new ArgumentException("ID da reserva deve ser maior que zero.");

                // Busca a reserva para validar se existe e se já não está desativada
                var existingReservation = await _unitOfWork.ReservationRepository.GetByIdAsync(id);
                if (existingReservation == null)
                    throw new KeyNotFoundException("Reserva não encontrada para desativação.");

                // Verifica se a reserva já está desativada
                if (!existingReservation.IsActive)
                    throw new InvalidOperationException("A reserva já está desativada.");

                // Verifica se a reserva pode ser desativada (ex: não está finalizada)
                if (existingReservation.Status?.ToLower() == "completed")
                    throw new InvalidOperationException("Não é possível desativar uma reserva que já foi finalizada.");

                // Chama o repository para desativar
                var deactivatedReservation = await _unitOfWork.ReservationRepository.DeactivateAsync(id);
                
                // Atualiza o status da reserva para "cancelled" quando desativada
                deactivatedReservation.Status = "cancelled";
                await _unitOfWork.ReservationRepository.UpdateAsync(deactivatedReservation);
                await _unitOfWork.SaveAsync();

                return deactivatedReservation;

            }, "desativação de reserva");
        }
    }
}
