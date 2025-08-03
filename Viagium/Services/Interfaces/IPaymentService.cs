using Viagium.EntitiesDTO.ApiDTO;
using Viagium.EntitiesDTO.User;
using Viagium.Models;

namespace Viagium.Services.Interfaces;

public interface IPaymentService
{
    Task<Payment> AddPaymentAsync(Reservation reservation);
    Task<string> CreateUserAsync(AsaasUserDTO user);
    Task<string> GetBankSlipByDocumentNumber(string documentNumber);
    Task<string> GetPixQrCodeByCpfAsync(string documentNumber);
    Task<Payment?> GetPaymentByIdAsync(int paymentId);
    Task SynchronizePaymentsAsync();
}