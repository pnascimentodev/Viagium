using Viagium.Services.Interfaces;
using Viagium.Repository.Interface;
using Viagium.EntitiesDTO.TravelPackageDTO;
using AutoMapper;

namespace Viagium.Services;

/// <summary>
/// Serviço em background para monitorar e atualizar automaticamente os campos 
/// ConfirmedPeople e IsAvailable dos pacotes de viagem
/// </summary>
public class TravelPackageAvailabilityBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TravelPackageAvailabilityBackgroundService> _logger;
    private readonly TimeSpan _checkInterval;

    public TravelPackageAvailabilityBackgroundService(
        IServiceProvider serviceProvider, 
        ILogger<TravelPackageAvailabilityBackgroundService> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        
        // Intervalo configurável: padrão 5 minutos para verificação de disponibilidade
        var intervalMinutes = configuration["TRAVEL_PACKAGE_AVAILABILITY_CHECK_INTERVAL_MINUTES"];
        _checkInterval = TimeSpan.FromMinutes(int.TryParse(intervalMinutes, out var minutes) ? minutes : 1);
        
        _logger.LogInformation($"🎯 Serviço de monitoramento de disponibilidade de pacotes iniciado. Intervalo: {_checkInterval.TotalMinutes} minutos");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Aguarda 45 segundos antes de iniciar para garantir que outros serviços estejam prontos
        await Task.Delay(TimeSpan.FromSeconds(45), stoppingToken);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("⏰ Iniciando verificação de disponibilidade dos pacotes de viagem...");
                
                using var scope = _serviceProvider.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                
                await UpdateTravelPackageAvailabilityAsync(unitOfWork, mapper);
                
                _logger.LogInformation($"✅ Verificação de disponibilidade concluída. Próxima em {_checkInterval.TotalMinutes} minutos.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro durante verificação de disponibilidade dos pacotes");
            }

            try
            {
                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
        
        _logger.LogInformation("🛑 Serviço de monitoramento de disponibilidade de pacotes interrompido.");
    }

    /// <summary>
    /// Atualiza o ConfirmedPeople e IsAvailable de todos os pacotes de viagem ativos
    /// </summary>
    private async Task UpdateTravelPackageAvailabilityAsync(IUnitOfWork unitOfWork, IMapper mapper)
    {
        try
        {
            // 1. Buscar todos os pacotes de viagem ativos
            var allPackages = await unitOfWork.TravelPackageRepository.ListAllActiveAsync();
            
            if (!allPackages.Any())
            {
                _logger.LogDebug("📭 Nenhum pacote de viagem ativo encontrado.");
                return;
            }

            int totalPackagesUpdated = 0;
            int totalPackagesUnavailable = 0;

            foreach (var package in allPackages)
            {
                try
                {
                    var (updated, unavailable) = await UpdateSinglePackageAvailabilityAsync(
                        unitOfWork, 
                        mapper, 
                        package.TravelPackageId);
                    
                    totalPackagesUpdated += updated;
                    totalPackagesUnavailable += unavailable;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $" Erro ao processar pacote {package.TravelPackageId}");
                }
            }

            // 3. Salvar todas as alterações apenas se houver atualizações
            if (totalPackagesUpdated > 0)
            {
                await unitOfWork.SaveAsync();
                _logger.LogInformation($" Estatísticas de atualização:");
                _logger.LogInformation($"   - Pacotes atualizados: {totalPackagesUpdated}");
                _logger.LogInformation($"   - Pacotes indisponíveis: {totalPackagesUnavailable}");
            }
            else
            {
                _logger.LogDebug(" Nenhum pacote precisou ser atualizado.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, " Erro geral ao atualizar disponibilidade dos pacotes");
        }
    }

    /// <summary>
    /// Atualiza um pacote específico
    /// </summary>
    private async Task<(int packagesUpdated, int packagesUnavailable)> UpdateSinglePackageAvailabilityAsync(
        IUnitOfWork unitOfWork, 
        IMapper mapper, 
        int travelPackageId)
    {
        int packagesUpdated = 0;
        int packagesUnavailable = 0;
        
        // 1. Buscar o pacote específico
        var travelPackage = await unitOfWork.TravelPackageRepository.GetByIdAsync(travelPackageId);
        if (travelPackage == null)
        {
            _logger.LogWarning($"⚠️ Pacote {travelPackageId} não encontrado");
            return (packagesUpdated, packagesUnavailable);
        }

        // 2. Buscar todas as reservas ativas e confirmadas deste pacote
        var allReservations = await unitOfWork.ReservationRepository.GetAllAsync();
        var confirmedReservations = allReservations
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
            var travelers = await unitOfWork.TravelerRepository.GetByReservationIdAsync(reservation.ReservationId);
            peopleInReservation += travelers.Count();
            
            totalConfirmedPeople += peopleInReservation;
            
            _logger.LogInformation($"   Reserva {reservation.ReservationId}: 1 usuário + {travelers.Count()} viajantes = {peopleInReservation} pessoas");
        }

        // 4. Verificar se houve mudança
        var currentConfirmedPeople = travelPackage.ConfirmedPeople;
        var currentIsAvailable = travelPackage.IsAvailable;
        
        // Calcular nova disponibilidade
        var newIsAvailable = totalConfirmedPeople < travelPackage.MaxPeople;

        // 5. Atualizar apenas se houver mudança
        if (currentConfirmedPeople != totalConfirmedPeople || currentIsAvailable != newIsAvailable)
        {
            _logger.LogInformation($" Atualizando pacote {travelPackageId}:");
            _logger.LogInformation($"   - ConfirmedPeople: {currentConfirmedPeople} → {totalConfirmedPeople}");
            _logger.LogInformation($"   - IsAvailable: {currentIsAvailable} → {newIsAvailable}");
            _logger.LogInformation($"   - MaxPeople: {travelPackage.MaxPeople}");
            _logger.LogInformation($"   - Vagas restantes: {travelPackage.MaxPeople - totalConfirmedPeople}");

            // Atualizar campos
            travelPackage.ConfirmedPeople = totalConfirmedPeople;
            travelPackage.IsAvailable = newIsAvailable;

            // Converter para DTO e atualizar
            var responseTravelPackageDTO = mapper.Map<ResponseTravelPackageDTO>(travelPackage);
            responseTravelPackageDTO.ConfirmedPeople = totalConfirmedPeople;
            responseTravelPackageDTO.IsAvailable = newIsAvailable;
            
            await unitOfWork.TravelPackageRepository.UpdateAsync(responseTravelPackageDTO);
            
            packagesUpdated++;

            if (!newIsAvailable)
            {
                packagesUnavailable++;
                _logger.LogWarning($" Pacote {travelPackageId} ficou INDISPONÍVEL (lotação esgotada)");
            }
            else if (!currentIsAvailable && newIsAvailable)
            {
                _logger.LogInformation($" Pacote {travelPackageId} ficou DISPONÍVEL novamente");
            }
        }
        else
        {
            _logger.LogDebug($" Pacote {travelPackageId} não precisou ser atualizado");
        }
        
        return (packagesUpdated, packagesUnavailable);
    }

    /// <summary>
    /// Força a atualização de um pacote específico (pode ser chamado externamente)
    /// </summary>
    public async Task ForceUpdatePackageAsync(int travelPackageId)
    {
        try
        {
            _logger.LogInformation($" Forçando atualização do pacote {travelPackageId}...");
            
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
            
            var (packagesUpdated, packagesUnavailable) = await UpdateSinglePackageAvailabilityAsync(
                unitOfWork, 
                mapper, 
                travelPackageId);
            
            if (packagesUpdated > 0)
            {
                await unitOfWork.SaveAsync();
                _logger.LogInformation($" Pacote {travelPackageId} atualizado com sucesso");
            }
            else
            {
                _logger.LogInformation($" Pacote {travelPackageId} não precisou ser atualizado");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $" Erro ao forçar atualização do pacote {travelPackageId}");
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(" Parando serviço de monitoramento de disponibilidade de pacotes...");
        await base.StopAsync(stoppingToken);
    }
}
