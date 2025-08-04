using Viagium.Services.Interfaces;
using Viagium.Repository.Interface;
using Viagium.Models.ENUM;
using Viagium.EntitiesDTO.Email;

namespace Viagium.Services;

/// <summary>
/// Serviço em background para monitorar e atualizar automaticamente o status das reservas
/// e disparar ações automáticas como envio de emails de review
/// </summary>
public class ReservationStatusBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ReservationStatusBackgroundService> _logger;
    private readonly TimeSpan _checkInterval;
    private readonly Dictionary<int, string> _lastKnownStatuses = new();

    public ReservationStatusBackgroundService(
        IServiceProvider serviceProvider, 
        ILogger<ReservationStatusBackgroundService> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        
        // Intervalo configurável: padrão 1 minuto para monitoramento em tempo real
        var intervalMinutes = configuration["RESERVATION_STATUS_CHECK_INTERVAL_MINUTES"];
        _checkInterval = TimeSpan.FromMinutes(int.TryParse(intervalMinutes, out var minutes) ? minutes : 1);
        
        _logger.LogInformation($"🏨 Serviço de monitoramento de reservas iniciado. Intervalo: {_checkInterval.TotalMinutes} minutos");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Aguarda 30 segundos antes de iniciar
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        
        // Carrega status inicial das reservas
        await LoadInitialReservationStatuses();
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogDebug("⏰ Verificando mudanças de status das reservas...");
                
                using var scope = _serviceProvider.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                var environment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
                
                await MonitorReservationStatusChanges(unitOfWork, emailService, environment);
                await UpdateExpiredReservationsAsync(unitOfWork, emailService, environment);
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro durante verificação de status das reservas");
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
        
        _logger.LogInformation("🛑 Serviço de monitoramento de reservas interrompido.");
    }

    /// <summary>
    /// Carrega o status inicial de todas as reservas ativas
    /// </summary>
    private async Task LoadInitialReservationStatuses()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            
            var allReservations = await unitOfWork.ReservationRepository.GetAllAsync();
            
            foreach (var reservation in allReservations.Where(r => r.IsActive))
            {
                _lastKnownStatuses[reservation.ReservationId] = reservation.Status ?? "unknown";
            }
            
            _logger.LogInformation($"📋 Status inicial carregado para {_lastKnownStatuses.Count} reservas");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao carregar status inicial das reservas");
        }
    }

    /// <summary>
    /// Monitora mudanças de status das reservas em tempo real
    /// </summary>
    private async Task MonitorReservationStatusChanges(IUnitOfWork unitOfWork, IEmailService emailService, IWebHostEnvironment environment)
    {
        var allReservations = await unitOfWork.ReservationRepository.GetAllAsync();
        var activeReservations = allReservations.Where(r => r.IsActive).ToList();
        
        foreach (var reservation in activeReservations)
        {
            var currentStatus = reservation.Status ?? "unknown";
            var reservationId = reservation.ReservationId;
            
            // Verifica se é uma nova reserva
            if (!_lastKnownStatuses.ContainsKey(reservationId))
            {
                _lastKnownStatuses[reservationId] = currentStatus;
                continue;
            }
            
            var previousStatus = _lastKnownStatuses[reservationId];
            
            // Detecta mudança de status
            if (previousStatus != currentStatus)
            {
                _logger.LogInformation($"🔄 Status mudou - Reserva {reservationId}: '{previousStatus}' → '{currentStatus}'");
                
                // Atualiza o status conhecido
                _lastKnownStatuses[reservationId] = currentStatus;
                
                // Executa ações baseadas na mudança de status
                await HandleStatusChange(reservation, previousStatus, currentStatus, emailService, environment, unitOfWork);
            }
        }
        
        // Remove reservas que não estão mais ativas
        var inactiveReservationIds = _lastKnownStatuses.Keys
            .Where(id => !activeReservations.Any(r => r.ReservationId == id))
            .ToList();
        
        foreach (var id in inactiveReservationIds)
        {
            _lastKnownStatuses.Remove(id);
        }
    }

    /// <summary>
    /// Executa ações específicas baseadas na mudança de status
    /// </summary>
    private async Task HandleStatusChange(
        Models.Reservation reservation, 
        string previousStatus, 
        string currentStatus,
        IEmailService emailService,
        IWebHostEnvironment environment,
        IUnitOfWork unitOfWork)
    {
        try
        {
            switch (currentStatus.ToLower())
            {
                case "confirmed":
                case "confirmado":
                    _logger.LogInformation($"🎉 RESERVA CONFIRMADA! ID: {reservation.ReservationId}");
                    break;
                    
                case "finished":
                case "finalizada":
                    _logger.LogInformation($"🏁 RESERVA FINALIZADA! ID: {reservation.ReservationId}");
                    await SendReviewRequestEmail(reservation, emailService, environment, unitOfWork);
                    break;
                    
                case "cancelled":
                case "cancelada":
                    _logger.LogInformation($"❌ RESERVA CANCELADA! ID: {reservation.ReservationId}");
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"❌ Erro ao processar mudança de status da reserva {reservation.ReservationId}");
        }
    }

    /// <summary>
    /// Atualiza reservas expiradas para status "finished" e envia emails de review
    /// </summary>
    private async Task UpdateExpiredReservationsAsync(IUnitOfWork unitOfWork, IEmailService emailService, IWebHostEnvironment environment)
    {
        try
        {
            var allReservations = await unitOfWork.ReservationRepository.GetAllAsync();
            var hoje = DateTime.Now.Date;
            int reservasFinalizadas = 0;

            foreach (var reserva in allReservations)
            {
                try
                {
                    // Verificar apenas reservas ativas e confirmadas
                    var statusLower = reserva.Status?.ToLower();
                    if (!reserva.IsActive || (statusLower != "confirmado" && statusLower != "confirmed"))
                        continue;

                    // Verificar se EndDate já passou
                    if (reserva.EndDate.Date < hoje)
                    {
                        // Verificar se tem pagamento confirmado
                        var hasConfirmedPayment = await HasConfirmedPaymentAsync(unitOfWork, reserva.ReservationId);
                        
                        if (hasConfirmedPayment)
                        {
                            var statusAnterior = reserva.Status;
                            
                            // Atualizar status para "finished"
                            reserva.Status = "finished";
                            await unitOfWork.ReservationRepository.UpdateAsync(reserva);
                            
                            // Atualizar status conhecido
                            _lastKnownStatuses[reserva.ReservationId] = "finished";
                            
                            reservasFinalizadas++;
                            
                            _logger.LogInformation($"🏁 Reserva {reserva.ReservationId} finalizada automaticamente:");
                            _logger.LogInformation($"   - EndDate: {reserva.EndDate:dd/MM/yyyy}");
                            _logger.LogInformation($"   - Status: {statusAnterior} → finished");
                            
                            // ✅ ENVIAR EMAIL DE REVIEW
                            await SendReviewRequestEmail(reserva, emailService, environment, unitOfWork);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"❌ Erro ao processar reserva {reserva.ReservationId}");
                }
            }

            // Salvar todas as alterações
            if (reservasFinalizadas > 0)
            {
                await unitOfWork.SaveAsync();
                _logger.LogInformation($"📊 {reservasFinalizadas} reservas finalizadas automaticamente");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Erro geral ao verificar reservas expiradas");
        }
    }

    /// <summary>
    /// Envia email de solicitação de review quando a reserva é finalizada
    /// </summary>
    private async Task SendReviewRequestEmail(
        Models.Reservation reservation,
        IEmailService emailService,
        IWebHostEnvironment environment,
        IUnitOfWork unitOfWork)
    {
        try
        {
            // Buscar dados do usuário se necessário
            if (reservation.User == null)
            {
                reservation.User = await unitOfWork.UserRepository.GetByIdAsync(reservation.UserId);
            }

            if (reservation.User == null)
            {
                _logger.LogWarning($"⚠️ Usuário não encontrado para envio de email de review. ReservationId: {reservation.ReservationId}");
                return;
            }

            var userEmail = reservation.User.Email;
            var userName = reservation.User.FirstName;

            // Carrega template de email
            var templatePath = Path.Combine(environment.ContentRootPath, "EmailTemplates", "User", "ReviewSolicited.html");
            
            if (!File.Exists(templatePath))
            {
                _logger.LogWarning($"⚠️ Template de email não encontrado: {templatePath}");
                return;
            }

            var template = await File.ReadAllTextAsync(templatePath);
            
            // ✅ SUBSTITUIR PLACEHOLDERS NO TEMPLATE
            var emailBody = template
                .Replace("{NOME}", userName)
                .Replace("{RESERVATION_ID}", reservation.ReservationId.ToString());
            
            var emailDto = new SendEmailDTO
            {
                To = userEmail,
                Subject = "✨ Conte-nos sobre sua experiência - Viagium",
                HtmlBody = emailBody
            };

            await emailService.SendEmailAsync(emailDto);

            _logger.LogInformation($"📧 Email de solicitação de review enviado para: {userEmail} (Reserva: {reservation.ReservationId})");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"❌ Erro ao enviar email de review para reserva {reservation.ReservationId}");
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

            return reserva.Payment.Status == PaymentStatus.RECEIVED;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"❌ Erro ao verificar pagamento da reserva {reservationId}");
            return false;
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🔄 Parando serviço de monitoramento de reservas...");
        await base.StopAsync(stoppingToken);
    }
}
