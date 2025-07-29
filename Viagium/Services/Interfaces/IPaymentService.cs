using Viagium.EntitiesDTO.ApiDTO;
using Viagium.EntitiesDTO.User;
using Viagium.Models;

namespace Viagium.Services.Interfaces;

public interface IPaymentService
{
    Task<Payment> AddAsync(Reservation reservation);
    Task<string> CreateUserAsync(AsaasUserDTO user);

}