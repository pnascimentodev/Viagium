namespace Viagium.Models.ENUM;

public enum PaymentStatus
{
    PENDING,   // Pagamento criado, aguardando
    RECEIVED,  // Pagamento confirmado/recebido
    OVERDUE,   // Pagamento vencido
    CANCELED   // Pagamento cancelado
}
