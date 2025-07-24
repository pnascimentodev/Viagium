using Viagium.Models;

namespace Viagium.Services.Interfaces;

public interface IAffiliateService
{
    Task<Affiliate> AddAsync(Affiliate affiliate);
    Task<Affiliate> UpdateAsync(Affiliate affiliate);
    Task<Affiliate> GetByIdAsync(int id);
    Task<IEnumerable<Affiliate>> GetAllAsync();
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<Affiliate>> GetByCityAsync(string city);
}