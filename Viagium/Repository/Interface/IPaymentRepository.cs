using Viagium.EntitiesDTO;

namespace Viagium.Repository.Interface;

public interface IPaymentRepository
{
    Task <PaymentDTO> CreatePaymentAsync(PaymentDTO payment);
    Task <PaymentDTO> GetPaymentByIdAsync(int paymentId);
    Task <PaymentDTO> UpdatePaymentAsync(PaymentDTO payment);
    Task <PaymentDTO> DeletePaymentAsync(int paymentId);
    Task <PaymentDTO> GetPaymentByReservationIdAsync(int reservationId);
    Task <PaymentDTO> RefundPaymentAsync(int paymentId);
}