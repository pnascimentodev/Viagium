using Viagium.Services.Interfaces;
using Viagium.Repository.Interface;
using Viagium.Models.ENUM;

namespace Viagium.Services;

/// <summary>
/// Serviço em background para atualizar automaticamente o status das reservas
/// quando EndDate ultrapassar a data atual (apenas para reservas com pagamento confirmado)
/// </summary>
public class ReservationStatusBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ReservationStatusBackgroundService> _logger;
    private readonly TimeSpan _checkInterval;

    public ReservationStatusBackgroundService(
        IServiceProvider serviceProvider, 
        ILogger<ReservationStatusBackgroundService> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        
        // Intervalo configurável: padrão 1 hora
        var intervalHours = configuration["RESERVATION_STATUS_CHECK_INTERVAL_HOURS"];
        _checkInterval = TimeSpan.FromHours(int.TryParse(intervalHours, out var hours) ? hours : 1);
        
        _logger.LogInformation($"🏨 Serviço de atualização de status de reservas iniciado. Intervalo: {_checkInterval.TotalHours} horas");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Aguarda 1 minuto antes de iniciar para garantir que a aplicação esteja totalmente inicializada
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("⏰ Iniciando verificação de status das reservas...");
                
                // Cria um novo escopo para resolver os serviços
                using var scope = _serviceProvider.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                
                await UpdateExpiredReservationsAsync(unitOfWork);
                
                _logger.LogInformation($"✅ Verificação concluída. Próxima em {_checkInterval.TotalHours} horas.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro durante verificação de status das reservas");
                // Não para o serviço, continua tentando nos próximos ciclos
            }

            try
            {
                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Cancellation foi solicitado, sair do loop
                break;
            }
        }
        
        _logger.LogInformation("🛑 Serviço de atualização de status de reservas interrompido.");
    }

    /// <summary>
    /// Atualiza reservas expiradas para status "Finished"
    /// </summary>
    private async Task UpdateExpiredReservationsAsync(IUnitOfWork unitOfWork)
    {
        try
        {
            Console.WriteLine("🔍 Verificando reservas expiradas...");
            
            // Buscar todas as reservas ativas
            var allReservations = await unitOfWork.ReservationRepository.GetAllAsync();
            
            var hoje = DateTime.Now.Date;
            int reservasVerificadas = 0;
            int reservasFinalizadas = 0;

            foreach (var reserva in allReservations)
            {
                try
                {
                    // Verificar apenas reservas ativas e confirmadas
                    if (!reserva.IsActive || reserva.Status != "Confirmado")
                        continue;

                    reservasVerificadas++;

                    // Verificar se EndDate já passou
                    if (reserva.EndDate.Date < hoje)
                    {
                        // Verificar se tem pagamento confirmado
                        var hasConfirmedPayment = await HasConfirmedPaymentAsync(unitOfWork, reserva.ReservationId);
                        
                        if (hasConfirmedPayment)
                        {
                            // Atualizar status para "Finished"
                            reserva.Status = "Finished";
                            await unitOfWork.ReservationRepository.UpdateAsync(reserva);
                            
                            reservasFinalizadas++;
                            
                            Console.WriteLine($"🏁 Reserva {reserva.ReservationId} finalizada automaticamente:");
                            Console.WriteLine($"   - EndDate: {reserva.EndDate:dd/MM/yyyy}");
                            Console.WriteLine($"   - Status anterior: Confirmado → Finished");
                            Console.WriteLine($"   - Pagamento: Confirmado");
                        }
                        else
                        {
                            Console.WriteLine($"⚠️ Reserva {reserva.ReservationId} expirada mas sem pagamento confirmado - mantendo status atual");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Erro ao processar reserva {reserva.ReservationId}: {ex.Message}");
                    // Continue processando outras reservas mesmo se uma falhar
                }
            }

            // Salvar todas as alterações de uma vez
            if (reservasFinalizadas > 0)
            {
                await unitOfWork.SaveAsync();
            }
            
            Console.WriteLine($"📊 Verificação de reservas concluída:");
            Console.WriteLine($"   - Reservas verificadas: {reservasVerificadas}");
            Console.WriteLine($"   - Reservas finalizadas: {reservasFinalizadas}");
            Console.WriteLine($"   - Data/hora da verificação: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 Erro geral ao verificar reservas expiradas: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Verifica se a reserva tem um pagamento confirmado
    /// </summary>
    private async Task<bool> HasConfirmedPaymentAsync(IUnitOfWork unitOfWork, int reservationId)
    {
        try
        {
            var reserva = await unitOfWork.ReservationRepository.GetByIdWithPaymentAsync(reservationId);
            
            if (reserva?.Payment == null)
                return false;

            // Verifica se o pagamento está confirmado (RECEIVED)
            return reserva.Payment.Status == PaymentStatus.RECEIVED;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro ao verificar pagamento da reserva {reservationId}: {ex.Message}");
            return false;
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🔄 Parando serviço de atualização de status de reservas...");
        await base.StopAsync(stoppingToken);
    }
}
