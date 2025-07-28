using Viagium.Models;
using Viagium.EntitiesDTO;
using Viagium.EntitiesDTO.Affiliate;

namespace Viagium.Services.Interfaces;

public interface IAffiliateService
{
    // ÚNICO método de criação - mais robusto e completo
    Task<AffiliateDTO> AddAsync(AffiliateCreateDto affiliateCreateDto, string password);
    Task<Affiliate> UpdateAsync(Affiliate affiliate);
    Task<Affiliate> GetByIdAsync(int id);
    Task<IEnumerable<Affiliate>> GetAllAsync();
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<Affiliate>> GetByCityAsync(string city);
    Task<AffiliateDTO> GetByEmailAsync(string email, bool includeDeleted = false);
}