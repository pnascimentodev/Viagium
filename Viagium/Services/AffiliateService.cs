using System.ComponentModel.DataAnnotations;
using System.Data;
using Viagium.Models;
using Viagium.Repository.Interface;
using Viagium.Services.Interfaces;

namespace Viagium.Services;

public class AffiliateService : IAffiliateService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public AffiliateService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    
    public Task<Affiliate> AddAsync(Affiliate affiliate)
    {
        var validationContext = new ValidationContext(affiliate);
        Validator.ValidateObject(affiliate, validationContext, validateAllProperties: true);

        ValidadeCustomRules(affiliate);
        
        return ExceptionHandler.ExecuteWithHandling(async () =>
        {
            
            await _unitOfWork.AffiliateRepository.AddAsync(affiliate);
            await _unitOfWork.SaveAsync();
            
            return affiliate;
        }, "criação de afiliado");
        
    }

    public Task<Affiliate> UpdateAsync(Affiliate affiliate)
    {
        var validationContext = new ValidationContext(affiliate);
        Validator.ValidateObject(affiliate, validationContext, validateAllProperties: true);

        HasFieldUpdatedToNullOrEmpty(affiliate);
        
        return ExceptionHandler.ExecuteWithHandling(async () =>
        {
            await _unitOfWork.AffiliateRepository.UpdateAsync(affiliate);
            await _unitOfWork.SaveAsync();
            
            return affiliate;
        }, "atualização de afiliado");
    }

    public Task<Affiliate> GetByIdAsync(int id)
    {
        return ExceptionHandler.ExecuteWithHandling(async () =>
        {
            var affiliate = await _unitOfWork.AffiliateRepository.GetByIdAsync(id);
            if (affiliate == null)
                throw new KeyNotFoundException("Afiliado não encontrado.");
            
            return affiliate;
        }, "busca de afiliado");
    }

    public Task<IEnumerable<Affiliate>> GetAllAsync()
    {
        return ExceptionHandler.ExecuteWithHandling(async () =>
        {
            var affiliates = await _unitOfWork.AffiliateRepository.GetAllAsync();
            
            return affiliates;
        }, "busca de afiliados");
    }

    public Task<bool> DeleteAsync(int id)
    {
        return ExceptionHandler.ExecuteWithHandling(async()=> 
            {
            var affiliate = await _unitOfWork.AffiliateRepository.GetByIdAsync(id);
            
            if (affiliate == null)
                throw new KeyNotFoundException("Afiliado não encontrado.");
            if (!affiliate.IsActive) throw new InvalidOperationException("Afiliado já está inativo.");
            
            affiliate.DeletedAt = DateTime.Now;
            affiliate.IsActive = false;
            
            await _unitOfWork.AffiliateRepository.UpdateAsync(affiliate);
            await _unitOfWork.SaveAsync();
            
            return true;
        }, "exclusão de afiliado");
    }
    
    private void ValidadeCustomRules(Affiliate affiliate)
    {
        
        var errors = new List<string>();
        
        if(affiliate.Cnpj == "00000000000000" || affiliate.Cnpj == _unitOfWork.AffiliateRepository.GetByCnpjAsync(affiliate.Cnpj).Result?.Cnpj )
            errors.Add("CNPJ inválido ou já cadastrado.");
        
        if(affiliate.StateRegistration == "00000000000000" || affiliate.StateRegistration == _unitOfWork.AffiliateRepository.GetByStateRegistrationAsync(affiliate.StateRegistration).Result?.StateRegistration )
            errors.Add("CNPJ inválido ou já cadastrado.");
        
        
        if (errors.Any())
            throw new ArgumentException(string.Join("\n", errors));
    }
    
private bool HasFieldUpdatedToNullOrEmpty(Affiliate updated)
{
    if (updated == null)
        throw new NoNullAllowedException("Os dados do afiliado não podem ser nulos.");

    // Verifica se algum campo foi atualizado para null ou vazio
    if (string.IsNullOrEmpty(updated.Name)) return true;
    if (string.IsNullOrEmpty(updated.Cnpj)) return true;
    if (string.IsNullOrEmpty(updated.CompanyName)) return true;
    if (string.IsNullOrEmpty(updated.Email)) return true;
    if (string.IsNullOrEmpty(updated.Phone)) return true;
    if (string.IsNullOrEmpty(updated.StateRegistration)) return true;
    if (string.IsNullOrEmpty(updated.HashPassword)) return true;
    // Campos de datas e coleções normalmente não são atualizados manualmente
    return false;
}
    
    
    

}