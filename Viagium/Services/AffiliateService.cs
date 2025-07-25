using System.ComponentModel.DataAnnotations;
using System.Data;
using AutoMapper;
using Viagium.Models;
using Viagium.Repository.Interface;
using Viagium.Services.Interfaces;
using Viagium.EntitiesDTO;
using Viagium.EntitiesDTO.Affiliate;

namespace Viagium.Services;

public class AffiliateService : IAffiliateService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    
    public AffiliateService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }
    
    // Método principal de criação - ÚNICO método AddAsync
    public async Task<AffiliateDTO> AddAsync(AffiliateCreateDto affiliateCreateDto, string password)
    {
        return await ExceptionHandler.ExecuteWithHandling(async () =>
        {
            // Verifica se já existe afiliado com o mesmo e-mail
            if (await _unitOfWork.AffiliateRepository.EmailExistsAsync(affiliateCreateDto.Email))
                throw new ArgumentException("Já existe um afiliado cadastrado com este e-mail.");

            // Validação da senha antes do hash
            ValidatePassword(password);

            // Verifica se já existe afiliado com o mesmo CNPJ
            if (await _unitOfWork.AffiliateRepository.CnpjExistsAsync(affiliateCreateDto.Cnpj))
                throw new ArgumentException("Já existe um afiliado cadastrado com este CNPJ.");

            // Validação se o afiliado está conforme o contrato do DTO
            var validationContext = new ValidationContext(affiliateCreateDto);
            Validator.ValidateObject(affiliateCreateDto, validationContext, validateAllProperties: true);

            // Primeiro, cria o endereço obrigatório
            var address = new Address
            {
                StreetName = affiliateCreateDto.Address.StreetName,
                AddressNumber = affiliateCreateDto.Address.AddressNumber,
                Neighborhood = affiliateCreateDto.Address.Neighborhood,
                City = affiliateCreateDto.Address.City,
                State = affiliateCreateDto.Address.State,
                ZipCode = affiliateCreateDto.Address.ZipCode,
                Country = affiliateCreateDto.Address.Country,
                CreatedAt = DateTime.Now
            };

            // Adiciona o endereço primeiro
            await _unitOfWork.AddressRepository.AddAsync(address);
            await _unitOfWork.SaveAsync(); // Salva para obter o ID do endereço

            // Agora cria o afiliado com o ID do endereço
            var affiliate = new Affiliate
            {
                Name = affiliateCreateDto.Name,
                Cnpj = affiliateCreateDto.Cnpj,
                CompanyName = affiliateCreateDto.CompanyName,
                Email = affiliateCreateDto.Email,
                Phone = affiliateCreateDto.Phone,
                StateRegistration = affiliateCreateDto.StateRegistration,
                HashPassword = PasswordHelper.HashPassword(password),
                NumberCadastur = affiliateCreateDto.NumberCadastur,
                ExpirationDate = affiliateCreateDto.ExpirationDate,
                IsActiveCadastur = affiliateCreateDto.IsActiveCadastur,
                CreatedAt = DateTime.Now, // Sempre usar DateTime.Now
                IsActive = true, // Sempre ativo na criação
                AddressId = address.AdressId
            };

            // Validações customizadas específicas do negócio
            ValidateCustomRules(affiliate);

            // Adiciona o afiliado
            await _unitOfWork.AffiliateRepository.AddAsync(affiliate);
            await _unitOfWork.SaveAsync();
            
            // Mapeamento para AffiliateDTO
            return new AffiliateDTO
            {
                AffiliateId = affiliate.AffiliateId,
                Name = affiliate.Name,
                Cnpj = affiliate.Cnpj,
                CompanyName = affiliate.CompanyName,
                Email = affiliate.Email,
                Phone = affiliate.Phone,
                StateRegistration = affiliate.StateRegistration,
                HashPassword = affiliate.HashPassword,
                NumberCadastur = affiliate.NumberCadastur,
                ExpirationDate = affiliate.ExpirationDate,
                IsActiveCadastur = affiliate.IsActiveCadastur,
                CreatedAt = affiliate.CreatedAt,
                AddressId = affiliate.AddressId,
                Address = new AddressDTO
                {
                    AddressId = address.AdressId,
                    StreetName = address.StreetName,
                    AddressNumber = address.AddressNumber,
                    Neighborhood = address.Neighborhood,
                    City = address.City,
                    State = address.State,
                    ZipCode = address.ZipCode,
                    Country = address.Country,
                    CreatedAt = address.CreatedAt,
                },
               
            };
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
    
    public async Task<IEnumerable<Affiliate>> GetByCityAsync(string city)
    {
        return await _unitOfWork.AffiliateRepository.GetByCityAsync(city);
    }
    
    public async Task<AffiliateDTO?> GetByEmailAsync(string email, bool unused)
    {
        return await ExceptionHandler.ExecuteWithHandling(async () =>
        {
            // Validação básica do parâmetro de entrada
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email é obrigatório para busca.");

            var affiliate = await _unitOfWork.AffiliateRepository.GetByEmailAsync(email);
        
            // Se não encontrar, simplesmente retorna null (comportamento esperado)
            if (affiliate == null)
                throw new ArgumentException("Email não encontrado.");

            return _mapper.Map<AffiliateDTO>(affiliate);
        }, "buscar usuário por e-mail");
    }

    private void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("A senha é obrigatória.");

        if (password.Length < 8)
            throw new ArgumentException("A senha deve ter pelo menos 6 caracteres.");

        if (password.Length > 100)
            throw new ArgumentException("A senha não pode ter mais de 100 caracteres.");

        // Verifica se tem pelo menos uma letra
        if (!password.Any(char.IsLetter))
            throw new ArgumentException("A senha deve conter pelo menos uma letra.");

        // Verifica se tem pelo menos um número
        if (!password.Any(char.IsDigit))
            throw new ArgumentException("A senha deve conter pelo menos um número.");
    }

    private void ValidateCustomRules(Affiliate affiliate)
    {
        var errors = new List<string>();
        
        if(affiliate.Cnpj == "00000000000000" || affiliate.Cnpj == _unitOfWork.AffiliateRepository.GetByCnpjAsync(affiliate.Cnpj).Result?.Cnpj )
            errors.Add("CNPJ inválido ou já cadastrado.");
        
        if(affiliate.StateRegistration == "00000000000000" || affiliate.StateRegistration == _unitOfWork.AffiliateRepository.GetByStateRegistrationAsync(affiliate.StateRegistration).Result?.StateRegistration )
            errors.Add("Inscrição Estadual inválida ou já cadastrada.");
        
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