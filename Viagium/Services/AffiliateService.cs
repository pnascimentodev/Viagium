using System.ComponentModel.DataAnnotations;
using System.Data;
using AutoMapper;
using Viagium.Models;
using Viagium.Repository.Interface;
using Viagium.Services.Interfaces;
using Viagium.EntitiesDTO;
using Viagium.EntitiesDTO.Affiliate;
using Viagium.EntitiesDTO.Email;
using Viagium.EntitiesDTO.User;

namespace Viagium.Services;

public class AffiliateService : IAffiliateService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IEmailService _emailService;
    
    public AffiliateService(IUnitOfWork unitOfWork, IMapper mapper, IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _emailService = emailService;
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
                CreatedAt = DateTime.Now, // Sempre usar DateTime.Now
                IsActive = true, // Sempre ativo na criação
                AddressId = address.AdressId,
                Address = null // Garante que não será criado novo Address
            };

            // Validações customizadas específicas do negócio
            ValidateCustomRules(affiliate);

            // Adiciona o afiliado
            await _unitOfWork.AffiliateRepository.AddAsync(affiliate);
            await _unitOfWork.SaveAsync();

            // Atualiza o endereço com o AffiliateId gerado
            address.AffiliateId = affiliate.AffiliateId;
            await _unitOfWork.AddressRepository.UpdateAsync(address);
            await _unitOfWork.SaveAsync();

            // Envia e-mail de boas-vindas para o afiliado
            var htmlBody = File.ReadAllText("EmailTemplates/Affiliate/WelcomeAffiliate.html");
            htmlBody = htmlBody.Replace("{NOME}", affiliate.Name);
            var emailDto = new SendEmailDTO
            {
                To = affiliate.Email,
                Subject = "Bem-vindo ao Viagium!",
                HtmlBody = htmlBody
            };
            await _emailService.SendEmailAsync(emailDto);
            
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


    public async Task<AffiliateDTO> GetByIdAsync(int id)
    {
        return await ExceptionHandler.ExecuteWithHandling(async () =>
        {
            var affiliate = await _unitOfWork.AffiliateRepository.GetByIdAsync(id);
            if (affiliate == null)
                throw new KeyNotFoundException("Afiliado não encontrado.");
            return _mapper.Map<AffiliateDTO>(affiliate);
        }, "busca de afiliado");
    }

    public async Task<IEnumerable<AffiliateDTO>> GetAllAsync()
    {
        return await ExceptionHandler.ExecuteWithHandling(async () =>
        {
            var affiliates = await _unitOfWork.AffiliateRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<AffiliateDTO>>(affiliates);
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
    
    public async Task<AffiliateDTO> GetByEmailAsync(string email, bool includeDeleted = false)
    {
        return await ExceptionHandler.ExecuteWithHandling(async () =>
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email é obrigatório para busca.");
            var affiliate = await _unitOfWork.AffiliateRepository.GetByEmailAsync(email);
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
    
    public async Task SendForgotPasswordEmailAsync(string email)
    {
        var affiliate = await _unitOfWork.AffiliateRepository.GetEmailByForgotPasswordAsync(email);
        if (affiliate == null)
            throw new ArgumentException("Não foi encontrado nenhum afiliado com este e-mail. Valide seus dados.");
        
        var htmlBody = File.ReadAllText("EmailTemplates/Affiliate/ForgorPasswordAffiliate.html");
        htmlBody = htmlBody.Replace("{NOME}", affiliate.Name)
            .Replace("{ID}", affiliate.AffiliateId.ToString());
        
        var emailDto = new SendEmailDTO
        {
            To = affiliate.Email,
            Subject = "Recuperação de senha de Afiliado- Viagium",
            HtmlBody = htmlBody
        };
        await _emailService.SendEmailAsync(emailDto);
    }
    
    public async Task<AffiliateDTO> UpdatePasswordAsync(int id, UpdatePasswordDto dto)
    {
        var affiliate = await _unitOfWork.AffiliateRepository.UpdatePasswordAsync(id, dto.NewPassword);
        if (affiliate == null)
            throw new KeyNotFoundException("Afiliado não encontrado para troca de senha.");
        // Envia e-mail de confirmação de alteração de senha
        var htmlBody = File.ReadAllText("EmailTemplates/Affiliate/SucessPasswordAffiliate.html");
        htmlBody = htmlBody.Replace("{NOME}", affiliate.Name);
        htmlBody = htmlBody.Replace("{DATA}", DateTime.Now.ToString("dd/MM/yyyy"));
        htmlBody = htmlBody.Replace("{HORA}", DateTime.Now.ToString("HH:mm"));
        var emailDto = new SendEmailDTO
        {
            To = affiliate.Email,
            Subject = "Senha alterada com sucesso - Viagium",
            HtmlBody = htmlBody
        };
        await _emailService.SendEmailAsync(emailDto);
        return _mapper.Map<AffiliateDTO>(affiliate);
    }
    
    public async Task<AffiliateDTO> ForgotPasswordAsync(int id, string newPassword)
    {
        // Valida a senha
        ValidatePassword(newPassword);
        var affiliate = await _unitOfWork.AffiliateRepository.GetByIdAsync(id);
        if (affiliate == null)
            throw new KeyNotFoundException("Afiliado não encontrado para recuperação de senha.");

        // Faz o hash da nova senha
        affiliate.HashPassword = PasswordHelper.HashPassword(newPassword);
        await _unitOfWork.AffiliateRepository.UpdateAsync(affiliate);
        await _unitOfWork.SaveAsync();

        // Envia e-mail de sucesso de alteração de senha
        var htmlBody = File.ReadAllText("EmailTemplates/Affiliate/SucessPasswordAffiliate.html");
        htmlBody = htmlBody.Replace("{NOME_AFILIADO}", affiliate.Name)
                           .Replace("{NOME}", affiliate.Name)
                           .Replace("{DATA}", DateTime.Now.ToString("dd/MM/yyyy"))
                           .Replace("{HORA}", DateTime.Now.ToString("HH:mm"));
        var emailDto = new SendEmailDTO
        {
            To = affiliate.Email,
            Subject = "Senha alterada com sucesso - Viagium",
            HtmlBody = htmlBody
        };
        await _emailService.SendEmailAsync(emailDto);

        return _mapper.Map<AffiliateDTO>(affiliate);
    }
}
