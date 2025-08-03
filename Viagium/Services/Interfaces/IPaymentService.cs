using Viagium.EntitiesDTO.ApiDTO;
using Viagium.EntitiesDTO.Payment;
using Viagium.EntitiesDTO.User;
using Viagium.Models;
using Viagium.Models.ENUM;

namespace Viagium.Services.Interfaces;

public interface IPaymentService
{
    Task<Payment> AddPaymentAsync(int reservationId, PaymentMethodType paymentMethod);
    Task<Payment> AddPaymentAsync(int reservationId, PaymentMethodType paymentMethod, CreditCardDTO? creditCard = null, string? remoteIp = null);
    Task<string> CreateUserAsync(AsaasUserDTO user);
    Task<string> GetBankSlipByDocumentNumber(string documentNumber);
    Task<string> GetPixQrCodeByCpfAsync(string documentNumber);
    Task<Payment?> GetPaymentByIdAsync(int paymentId);
    Task SynchronizePaymentsAsync();
}