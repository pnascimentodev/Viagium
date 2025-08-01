using Viagium.Models;
using Viagium.EntitiesDTO;
using Viagium.EntitiesDTO.Affiliate;
using Viagium.EntitiesDTO.User;

namespace Viagium.Services.Interfaces;

public interface IAffiliateService
{
    // ÚNICO método de criação - mais robusto e completo
    Task<AffiliateDTO> AddAsync(AffiliateCreateDto affiliateCreateDto, string password);
    Task<Affiliate> UpdateAsync(Affiliate affiliate);
    Task<AffiliateDTO> GetByIdAsync(int id);
    Task<IEnumerable<AffiliateDTO>> GetAllAsync();
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<Affiliate>> GetByCityAsync(string city);
    Task<AffiliateDTO> GetByEmailAsync(string email, bool includeDeleted = false);
    Task SendForgotPasswordEmailAsync(string email);
    Task<AffiliateDTO> UpdatePasswordAsync(int id, UpdatePasswordDto dto);
    Task<AffiliateDTO> ForgotPasswordAsync(int id, string newPassword);
    Task<IEnumerable<AffiliateDTO>> GetAllAdmAsync(bool includeDeleted);

}