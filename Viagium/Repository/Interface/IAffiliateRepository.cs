using Viagium.Models;

namespace Viagium.Repository.Interface;

public interface IAffiliateRepository
{
    Task AddAsync(Affiliate affiliate);
    Task UpdateAsync(Affiliate affiliate);
    Task<Affiliate?> GetByIdAsync(int id);
    Task<IEnumerable<Affiliate>> GetAllAsync();
    Task<Affiliate?> GetByCnpjAsync(string cnpj);
    Task<Affiliate?> GetByStateRegistrationAsync(string stateRegistration);
    Task<IEnumerable<Affiliate>> GetByCityAsync(string city);
    Task<Affiliate?> GetByEmailAsync(string email, bool includeDeleted = false);
    Task<Affiliate?> GetEmailByForgotPasswordAsync (string email, bool includeDeleted = false);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> CnpjExistsAsync(string cnpj);
    Task<Affiliate> UpdatePasswordAsync (int id, string newPassword);
    Task<Affiliate?> ForgotPasswordAsync(int id, string newPassword);
    
}