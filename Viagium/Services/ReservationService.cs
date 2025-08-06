using AutoMapper;
using System.ComponentModel.DataAnnotations;
using Viagium.EntitiesDTO;
using Viagium.EntitiesDTO.Reservation;
using Viagium.EntitiesDTO.TravelPackageDTO;
using Viagium.Models;
using Viagium.Repository.Interface;
using Viagium.Services.Interfaces;
using Viagium.Data;
using Microsoft.EntityFrameworkCore;

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

                // Validar se o hotel existe antes de criar a reserva
                var hotelExists = await _unitOfWork.HotelRepository.GetByIdAsync(createReservationDto.HotelId);
                if (hotelExists == null)
                    throw new KeyNotFoundException($"Hotel com ID {createReservationDto.HotelId} não encontrado.");

                // ✅ CALCULAR PREÇO TOTAL AUTOMATICAMENTE
                reservation.TotalPrice = await CalculateTotalPriceAsync(createReservationDto);
                
                // ✅ CALCULAR DATAS AUTOMATICAMENTE BASEADO NO TRAVELPACKAGE
                await SetReservationDatesAsync(reservation, createReservationDto.TravelPackageId);
                
                reservation.IsActive = true; // Define a reserva como ativa
                reservation.HotelId = createReservationDto.HotelId; // Define o HotelId diretamente na reserva
                reservation.RoomTypeId = createReservationDto.RoomTypeId; // Define o RoomTypeId diretamente na reserva
                
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

                // Garantir que o RoomType também esteja presente
                if (dto.RoomType == null && createReservationDto.RoomTypeId > 0)
                {
                    var roomTypeData = await _unitOfWork.RoomTypeRepository.GetByIdAsync(createReservationDto.RoomTypeId);
                    if (roomTypeData != null)
                    {
                        dto.RoomType = _mapper.Map<RoomTypeDTO>(roomTypeData);
                    }
                }
                
                // Após criar a reserva, buscar o primeiro Room disponível do RoomType e marcar como indisponível
                var availableRoom = await _unitOfWork.RoomRepository.GetFirstAvailableByRoomTypeIdAsync(createReservationDto.RoomTypeId);
                if (availableRoom != null)
                {
                    availableRoom.IsAvailable = false;
                    await _unitOfWork.RoomRepository.UpdateAsync(availableRoom);
                }

                // ✅ CORREÇÃO: Removido a chamada problemática que pode causar loop infinito
                // await UpdateConfirmedPeopleCountAsync(createReservationDto.TravelPackageId);

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

                    // O RoomType agora vem automaticamente através do relacionamento direto
                    // Se não vier pelo AutoMapper, buscar manualmente como fallback
                    if (dto.RoomType == null && reservation.RoomTypeId.HasValue)
                    {
                        var roomTypeData = await _unitOfWork.RoomTypeRepository.GetByIdAsync(reservation.RoomTypeId.Value);
                        if (roomTypeData != null)
                        {
                            dto.RoomType = _mapper.Map<RoomTypeDTO>(roomTypeData);
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

                // O RoomType agora vem automaticamente através do relacionamento direto
                // Se não vier pelo AutoMapper, buscar manualmente como fallback
                if (dto.RoomType == null && reservation.RoomTypeId.HasValue)
                {
                    var roomTypeData = await _unitOfWork.RoomTypeRepository.GetByIdAsync(reservation.RoomTypeId.Value);
                    if (roomTypeData != null)
                    {
                        dto.RoomType = _mapper.Map<RoomTypeDTO>(roomTypeData);
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

            var paymentStatus = reservation.Payment.Status.ToString().ToLower();
            if (paymentStatus == "received" && reservation.Status != "Confirmado")
            {
                reservation.Status = "Confirmado";
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
                if (existingReservation.Status?.ToLower() == "finished")
                    throw new InvalidOperationException("Não é possível desativar uma reserva que já foi finalizada.");

                // Chama o repository para desativar
                var deactivatedReservation = await _unitOfWork.ReservationRepository.DeactivateAsync(id);
                
                // Atualiza o status da reserva para "cancelled" quando desativada
                deactivatedReservation.Status = "cancelled";
                await _unitOfWork.ReservationRepository.UpdateAsync(deactivatedReservation);
                await _unitOfWork.SaveAsync();

                // ✅ CORREÇÃO: Removido a chamada problemática que causa loop infinito
                // await UpdateConfirmedPeopleCountAsync(deactivatedReservation.TravelPackageId);

                return deactivatedReservation;

            }, "desativação de reserva");
        }


        public async Task<IEnumerable<ResponseReservationDTO>> GetByUserIdAsync(int userId)
        {
            return await ExceptionHandler.ExecuteWithHandling(async () =>
            {
                var reservations = await _unitOfWork.ReservationRepository.GetByUserIdAsync(userId);
                if (!reservations.Any())
                    throw new KeyNotFoundException("Nenhuma reserva encontrada para este usuário.");
                var dtos = new List<ResponseReservationDTO>();
                foreach (var reservation in reservations)
                {
                    var dto = _mapper.Map<ResponseReservationDTO>(reservation);
                    var travelers =
                        await _unitOfWork.TravelerRepository.GetByReservationIdAsync(reservation.ReservationId);
                    dto.Travelers = _mapper.Map<List<TravelerDTO>>(travelers);
                    dto.IsActive = reservation.IsActive;
                    if (dto.Hotel == null && reservation.HotelId.HasValue)
                    {
                        var hotelData = await _unitOfWork.HotelRepository.GetByIdAsync(reservation.HotelId.Value);
                        if (hotelData != null)
                        {
                            dto.Hotel = _mapper.Map<HotelDTO>(hotelData);
                        }
                    }

                    if (dto.RoomType == null && reservation.RoomTypeId.HasValue)
                    {
                        var roomTypeData =
                            await _unitOfWork.RoomTypeRepository.GetByIdAsync(reservation.RoomTypeId.Value);
                        if (roomTypeData != null)
                        {
                            dto.RoomType = _mapper.Map<RoomTypeDTO>(roomTypeData);
                        }
                    }

                    dtos.Add(dto);
                }

                return dtos;
            }, "busca reservas por usuário");
        }

        /// <summary>
        /// Calcula o preço total da reserva baseado no TravelPackage e RoomType
        /// Implementa a mesma lógica do frontend para consistência
        /// </summary>
        private async Task<decimal> CalculateTotalPriceAsync(CreateReservationDTO createReservationDto)
        {
            try
            {
                // 1. Buscar dados do TravelPackage
                var travelPackage = await _unitOfWork.TravelPackageRepository.GetByIdAsync(createReservationDto.TravelPackageId);
                if (travelPackage == null)
                    throw new KeyNotFoundException($"TravelPackage com ID {createReservationDto.TravelPackageId} não encontrado.");

                // 2. Buscar dados do RoomType
                var roomType = await _unitOfWork.RoomTypeRepository.GetByIdAsync(createReservationDto.RoomTypeId);
                if (roomType == null)
                    throw new KeyNotFoundException($"RoomType com ID {createReservationDto.RoomTypeId} não encontrado.");

                // 3. ✅ CORREÇÃO: Calcular número total de pessoas (usuário principal + viajantes adicionais)
                var numTravelersAdicionais = createReservationDto.Travelers?.Count ?? 0;
                var numPessoas = 1 + numTravelersAdicionais; // 1 (usuário principal) + viajantes adicionais

                // 4. Implementar lógica do frontend
                
                // const price = currentPackage ? currentPackage.price * numPessoas : 0;
                var price = travelPackage.OriginalPrice * numPessoas;

                // const packageTax = currentPackage ? currentPackage.packageTax : 0;
                var packageTax = travelPackage.PackageTax; // ✅ CORREÇÃO: PackageTax é decimal, não nullable

                // const discountPercent = currentPackage ? currentPackage.discountValue : 0;
                var discountPercent = travelPackage.DiscountValue; // ✅ CORREÇÃO: DiscountValue é decimal, não nullable

                // const pricePerNight = hotels[hotelIndex]?.roomTypes?.[roomTypeIndex]?.pricePerNight || 0;
                var pricePerNight = roomType.PricePerNight;

                // const durationNights = currentPackage ? (typeof currentPackage.duration === 'string' ? parseInt(currentPackage.duration) : Number(currentPackage.duration)) : 0;
                var durationNights = travelPackage.Duration;

                // const acomodationTotal = pricePerNight * (durationNights > 1 ? durationNights - 1 : 0) * numPessoas;
                var nightsToCharge = durationNights > 1 ? durationNights - 1 : 0;
                var acomodationTotal = pricePerNight * nightsToCharge * numPessoas;

                // const valorBase = price + packageTax + acomodationTotal;
                var valorBase = price + packageTax + acomodationTotal;

                // const valorDesconto = cupomAplicado && discountPercent > 0 ? (valorBase * (discountPercent / 100)) : 0;
                // Aplicar desconto automaticamente se existir
                var valorDesconto = discountPercent > 0 ? (valorBase * (discountPercent / 100)) : 0;

                // const valorFinal = valorBase - valorDesconto;
                var valorFinal = valorBase - valorDesconto;

                Console.WriteLine($" Cálculo de preço da reserva:");
                Console.WriteLine($"   - Usuário principal: 1 pessoa");
                Console.WriteLine($"   - Viajantes adicionais: {numTravelersAdicionais} pessoas");
                Console.WriteLine($"   - TOTAL DE PESSOAS: {numPessoas}");
                Console.WriteLine($"   - Preço base do pacote: R$ {price:F2} ({travelPackage.OriginalPrice:F2} x {numPessoas} pessoas)");
                Console.WriteLine($"   - Taxa do pacote: R$ {packageTax:F2}");
                Console.WriteLine($"   - Acomodação: R$ {acomodationTotal:F2} ({pricePerNight:F2}/noite x {nightsToCharge} noites x {numPessoas} pessoas)");
                Console.WriteLine($"   - Valor base: R$ {valorBase:F2}");
                Console.WriteLine($"   - Desconto ({discountPercent}%): R$ {valorDesconto:F2}");
                Console.WriteLine($"   - VALOR FINAL: R$ {valorFinal:F2}");

                return valorFinal;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"rro ao calcular preço total: {ex.Message}");
                throw new Exception($"Erro no cálculo do preço: {ex.Message}");
            }

        }

        /// <summary>
        /// Define as datas StartDate e EndDate da reserva baseado no PackageSchedule
        /// StartDate = PackageSchedule.StartDate (data programada do pacote)
        /// EndDate = StartDate + Duration do TravelPackage
        /// </summary>
        private async Task SetReservationDatesAsync(Reservation reservation, int travelPackageId)
        {
            try
            {
                // 1. Buscar dados do TravelPackage
                var travelPackage = await _unitOfWork.TravelPackageRepository.GetByIdAsync(travelPackageId);
                if (travelPackage == null)
                    throw new KeyNotFoundException($"TravelPackage com ID {travelPackageId} não encontrado.");

                // 2. Usar a data de início do próprio TravelPackage
                reservation.StartDate = travelPackage.StartDate.Date;

                // 3. Calcular EndDate baseado na duração do pacote
                // EndDate = StartDate + Duration (em dias)
                reservation.EndDate = reservation.StartDate.AddDays(travelPackage.Duration - 1);

                Console.WriteLine($"Datas da reserva calculadas:");
                Console.WriteLine($"   - Data de início: {reservation.StartDate:dd/MM/yyyy}");
                Console.WriteLine($"   - Data de fim: {reservation.EndDate:dd/MM/yyyy}");
                Console.WriteLine($"   - Duração total: {travelPackage.Duration} dias");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao calcular datas da reserva: {ex.Message}");
                throw new Exception($"Erro ao calcular datas da reserva: {ex.Message}");
            }
        }

        /// <summary>
        /// Atualiza automaticamente o número de pessoas confirmadas no TravelPackage
        /// baseado nas reservas ativas e confirmadas
        /// </summary>
        private async Task UpdateConfirmedPeopleCountAsync(int travelPackageId)
        {
            try
            {
                // 1. Buscar o TravelPackage
                var travelPackage = await _unitOfWork.TravelPackageRepository.GetByIdAsync(travelPackageId);
                if (travelPackage == null)
                {
                    Console.WriteLine($"⚠️ TravelPackage com ID {travelPackageId} não encontrado para atualizar contador de pessoas");
                    return;
                }

                // 2. Buscar todas as reservas ativas e confirmadas deste pacote
                var reservations = await _unitOfWork.ReservationRepository.GetAllAsync();
                var confirmedReservations = reservations
                    .Where(r => r.TravelPackageId == travelPackageId && 
                               r.IsActive && 
                               (r.Status?.ToLower() == "confirmado" || r.Status?.ToLower() == "confirmed"))
                    .ToList();

                // 3. Calcular o total de pessoas confirmadas
                int totalConfirmedPeople = 0;
                
                foreach (var reservation in confirmedReservations)
                {
                    // Contar o usuário principal (sempre 1 pessoa)
                    int peopleInReservation = 1;
                    
                    // Contar os viajantes adicionais
                    var travelers = await _unitOfWork.TravelerRepository.GetByReservationIdAsync(reservation.ReservationId);
                    peopleInReservation += travelers.Count();
                    
                    totalConfirmedPeople += peopleInReservation;
                    
                    Console.WriteLine($"   📊 Reserva {reservation.ReservationId}: 1 usuário + {travelers.Count()} viajantes = {peopleInReservation} pessoas");
                }

                // 4. Atualizar o contador no TravelPackage
                var currentCount = travelPackage.ConfirmedPeople;
                travelPackage.ConfirmedPeople = totalConfirmedPeople;
                // Atualizar disponibilidade
                travelPackage.IsAvailable = totalConfirmedPeople < travelPackage.MaxPeople;
                // 5. Salvar as alterações - converter para ResponseTravelPackageDTO
                var responseTravelPackageDTO = _mapper.Map<ResponseTravelPackageDTO>(travelPackage);
                responseTravelPackageDTO.ConfirmedPeople = totalConfirmedPeople;
                responseTravelPackageDTO.IsAvailable = travelPackage.IsAvailable;
                await _unitOfWork.TravelPackageRepository.UpdateAsync(responseTravelPackageDTO);
                await _unitOfWork.SaveAsync();

                Console.WriteLine($"Pessoas confirmadas no pacote {travelPackageId} atualizado:");
                Console.WriteLine($"   - Antes: {currentCount} pessoas");
                Console.WriteLine($"   - Depois: {totalConfirmedPeople} pessoas");
                Console.WriteLine($"   - Reservas confirmadas: {confirmedReservations.Count}");
                Console.WriteLine($"   - Vagas restantes: {travelPackage.MaxPeople - totalConfirmedPeople}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro ao atualizar contador de pessoas confirmadas: {ex.Message}");
                // Não relançar a exceção para não quebrar o fluxo principal de criação da reserva
            }
        }
    }
}
