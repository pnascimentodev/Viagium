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
}