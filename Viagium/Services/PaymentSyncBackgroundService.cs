using Viagium.Services.Interfaces;

namespace Viagium.Services;

/// <summary>
/// Serviço em background para sincronização automática de pagamentos
/// </summary>
public class PaymentSyncBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PaymentSyncBackgroundService> _logger;
    private readonly TimeSpan _syncInterval;

    public PaymentSyncBackgroundService(
        IServiceProvider serviceProvider, 
        ILogger<PaymentSyncBackgroundService> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        
        // Lê configuração do appsettings.json com fallback para 5 minutos
        var intervalMinutes = configuration["PAYMENT_SYNC_INTERVAL_MINUTES"];
        _syncInterval = TimeSpan.FromMinutes(int.TryParse(intervalMinutes, out var minutes) ? minutes : 5);
        
        _logger.LogInformation($"🔄 Serviço de sincronização iniciado. Intervalo: {_syncInterval.TotalMinutes} minutos");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Aguarda 30 segundos antes de iniciar para garantir que a aplicação esteja totalmente inicializada
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("⏰ Iniciando ciclo de sincronização automática...");
                
                // Cria um novo escopo para resolver os serviços
                using var scope = _serviceProvider.CreateScope();
                var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();
                
                await paymentService.SynchronizePaymentsAsync();
                
                _logger.LogInformation($"✅ Ciclo de sincronização concluído. Próximo em {_syncInterval.TotalMinutes} minutos.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro durante sincronização automática de pagamentos");
                // Não para o serviço, continua tentando nos próximos ciclos
            }

            try
            {
                await Task.Delay(_syncInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Cancellation foi solicitado, sair do loop
                break;
            }
        }
        
        _logger.LogInformation("🛑 Serviço de sincronização de pagamentos interrompido.");
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🔄 Parando serviço de sincronização de pagamentos...");
        await base.StopAsync(stoppingToken);
    }
}
