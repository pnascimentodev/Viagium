using Viagium.Models;

namespace Viagium.Repository.Interface;

public interface IPaymentRepository
{
    Task AddAsync(Payment payment);
    Task<Payment?> GetPaymentByIdAsync(int paymentId);
    Task<Payment?> GetByAsaasIdAsync(string asaasId);
    Task FinalizePaymentAsync(Payment payment);
}
