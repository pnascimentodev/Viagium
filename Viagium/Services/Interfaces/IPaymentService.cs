using Viagium.EntitiesDTO.ApiDTO;
using Viagium.EntitiesDTO.User;
using Viagium.Models;

namespace Viagium.Services.Interfaces;

public interface IPaymentService
{
    Task<Payment> AddPaymentAsync(AsaasPaymentDTO asaasPaymentDTO);
    Task<string> CreateUserAsync(AsaasUserDTO user);
    Task<string> GetBankSlipByDocumentNumber(string documentNumber);
    Task<string> GetPixQrCodeByCpfAsync(string documentNumber);

}