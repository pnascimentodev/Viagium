using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Viagium.EntitiesDTO.ApiDTO;
using Viagium.EntitiesDTO.User;
using Viagium.EntitiesDTO.Auth;
using Viagium.Models;
using Viagium.Services.Auth;
using Viagium.Services.Interfaces;
using Viagium.EntitiesDTO.Email;

using Viagium.Repository.Interface;

using User = Viagium.Models.User;

namespace Viagium.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;
    private readonly IMapper _mapper;
    private readonly PaymentService _paymentService;
    private readonly IEmailService _emailService;
    
    public UserService(IUnitOfWork unitOfWork, IAuthService authService, IEmailService emailService, PaymentService paymentService, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
        _emailService = emailService;
        _paymentService = paymentService;
        _mapper = mapper;

    }

    public async Task<UserDTO> AddAsync(UserCreateDTO userCreateDto, string password)
    {
        return await ExceptionHandler.ExecuteWithHandling(async () =>
        {
            // Verifica se já existe usuário com o mesmo e-mail
            if (await _unitOfWork.UserRepository.EmailExistsAsync(userCreateDto.Email))
                throw new ArgumentException("Já existe um usuário cadastrado com este e-mail.");

            // Validação da senha antes do hash
            ValidatePassword(password);

            // Verifica se já existe usuário com o mesmo número de documento
            if (await _unitOfWork.UserRepository.DocumentNumberExistsAsync(userCreateDto.DocumentNumber))
                throw new ArgumentException("Já existe um usuário cadastrado com este número de documento.");
            
            var asaasUser = _mapper.Map<AsaasUserDTO>(userCreateDto);
            var apiId = await _paymentService.CreateUserAsync(asaasUser);
            var user = new User
            {
                Email = userCreateDto.Email,
                FirstName = userCreateDto.FirstName,
                LastName = userCreateDto.LastName,
                DocumentNumber = userCreateDto.DocumentNumber,
                AsaasApiId = apiId,
                BirthDate = userCreateDto.BirthDate,
                HashPassword = PasswordHelper.HashPassword(password),
                Phone = userCreateDto.Phone,
                Role = (Role)1 // Define a role como 1
            };

            //validação se o usuario esta conforme o contrato do DTO
            var validationContext = new ValidationContext(userCreateDto);
            Validator.ValidateObject(userCreateDto, validationContext, validateAllProperties: true);

            // Validações customizadas específicas do negócio
            ValidadeCustomRules(user);

            // usando o UnitOfWork para adicionar o usuário
            await _unitOfWork.UserRepository.AddAsync(user);
            await _unitOfWork.SaveAsync();

            // Envia e-mail de boas-vindas
            var htmlBody = File.ReadAllText("EmailTemplates/WelcomeClient.html");
            var emailDto = new SendEmailDTO
            {
                To = user.Email,
                Subject = "Bem-vindo ao Viagium!",
                HtmlBody = htmlBody.Replace("{NOME}", user.FirstName)
            };
            await _emailService.SendEmailAsync(emailDto);
            // Retorno manual do DTO
            return new UserDTO
            {
                UserId = user.UserId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                DocumentNumber = user.DocumentNumber,
                BirthDate = user.BirthDate,
                Phone = user.Phone,
                Role = user.Role.ToString(),
                IsActive = user.IsActive,
                HashPassword = user.HashPassword,
                UpdatedAt = user.UpdatedAt ?? DateTime.Now
            };
        }, "criação de usuário");
        
        
    }

    public async Task<LoginResponseDTO> LoginAsync(LoginRequestDTO loginRequest)
    {
        if (string.IsNullOrWhiteSpace(loginRequest.Email) || string.IsNullOrWhiteSpace(loginRequest.Password))
            throw new ArgumentException("Email e senha são obrigatórios.");

        var user = await _unitOfWork.UserRepository.GetByEmailAsync(loginRequest.Email);
        if (user == null || !user.IsActive || user.DeletedAt != null)
            throw new UnauthorizedAccessException("Usuário ou senha inválidos.");

        if (!PasswordHelper.VerifyPassword(loginRequest.Password, user.HashPassword))
            throw new UnauthorizedAccessException("Usuário ou senha inválidos.");

        var token = _authService.GenerateJwtToken(user);
        return new LoginResponseDTO
        {
            Id = user.UserId.ToString(),
            Role = user.Role.ToString(),
            Token = token
        };
    }


    private void ValidadeCustomRules(User user)
    {
        var errors = new List<string>();

        if (user.BirthDate > DateTime.Now)
            errors.Add("Data de nascimento não pode ser futura.");

        if (errors.Any())
            throw new ArgumentException(string.Join("\n", errors));
    }

    public async Task<UserDTO?> GetByIdAsync(int id)
    {
        return await ExceptionHandler.ExecuteWithHandling(async () =>
        {
            var user = await _unitOfWork.UserRepository.GetByIdAsync(id);
            if (user == null) return null;
            return new UserDTO
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                DocumentNumber = user.DocumentNumber,
                BirthDate = user.BirthDate,
                Phone = user.Phone,
                Role = user.Role.ToString(),
                IsActive = user.IsActive,
                HashPassword = user.HashPassword
            };
        }, "buscar usuário por id");
    }

    public async Task<List<UserDTO>> GetAllAsync()
    {
        return await ExceptionHandler.ExecuteWithHandling(async () =>
        {
            var users = await _unitOfWork.UserRepository.GetAllAsync();
            return users.Select(user => new UserDTO
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                DocumentNumber = user.DocumentNumber,
                BirthDate = user.BirthDate,
                Phone = user.Phone,
                Role = user.Role.ToString(),
                IsActive = user.IsActive,
                HashPassword = user.HashPassword
            }).ToList();
        }, "buscar todos usuários");
    }

    public async Task UpdateAsync(UserUpdateDto userUpdateDto, string password)
    {
        await ExceptionHandler.ExecuteWithHandling(async () =>
        {
            if (await _unitOfWork.UserRepository.EmailExistsAsync(userUpdateDto.Email, userUpdateDto.UserId))
                throw new ArgumentException("Já existe outro usuário cadastrado com este e-mail.");

            ValidatePassword(password);

            var user = await _unitOfWork.UserRepository.GetByIdAsync(userUpdateDto.UserId);
            if (user == null)
                throw new KeyNotFoundException("Usuário não encontrado para atualização.");

            user.Email = userUpdateDto.Email;
            user.FirstName = userUpdateDto.FirstName;
            user.LastName = userUpdateDto.LastName;
            user.BirthDate = userUpdateDto.BirthDate;
            user.HashPassword = PasswordHelper.HashPassword(password);

            var validationContext = new ValidationContext(userUpdateDto);
            Validator.ValidateObject(userUpdateDto, validationContext, validateAllProperties: true);
            ValidadeCustomRules(user);
            await _unitOfWork.UserRepository.UpdateAsync(user);
            await _unitOfWork.SaveAsync();
        }, "atualizar usuário");
    }

    public async Task<User> DesativateAsync(int id)
    {
        return await ExceptionHandler.ExecuteWithHandling(async () =>
        {
            var user = await _unitOfWork.UserRepository.DesativateAsync(id);
            if (user == null)
                throw new KeyNotFoundException("Usuário não encontrado para desativação.");

            return user;
        }, "desativação de usuário");
    }

    public async Task<UserDTO> ActivateAsync(int id)
    {
        return await ExceptionHandler.ExecuteWithHandling(async () =>
        {
            var user = await _unitOfWork.UserRepository.ActivateAsync(id);
            if (user == null)
                throw new KeyNotFoundException("Usuário não encontrado para ativação.");

            return new UserDTO
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                DocumentNumber = user.DocumentNumber,
                BirthDate = user.BirthDate,
                Phone = user.Phone,
                Role = user.Role.ToString(),
                IsActive = user.IsActive,
                HashPassword = user.HashPassword
            };
        }, "ativação de usuário");
    }

    private void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            throw new ArgumentException("A senha precisa possuir pelo menos 8 caracteres.");
    }
    
    public async Task<UserDTO?> GetByEmailAsync(string email, bool unused)
    {
        return await ExceptionHandler.ExecuteWithHandling(async () =>
        {
            var user = await _unitOfWork.UserRepository.GetByEmailAsync(email);
            if (user == null)
            {
                // Validação DataAnnotation para email não encontrado
                var userEmailDto = new UserEmailDTO { Email = email };
                var context = new ValidationContext(userEmailDto);
                Validator.ValidateObject(userEmailDto, context, validateAllProperties: true);
                return null;
            }
            return _mapper.Map<UserDTO>(user);
        }, "buscar usuário por e-mail");
    }
    
    public async Task<UserDTO> UpdatePasswordAsync(int id, UpdatePasswordDto dto)
    {
        return await ExceptionHandler.ExecuteWithHandling(async () =>
        {
            ValidatePassword(dto.NewPassword);
            var user = await _unitOfWork.UserRepository.GetByIdAsync(id);
            if (user == null)
                throw new KeyNotFoundException("Usuário não encontrado para atualização de senha.");

            // Verifica se a senha antiga está correta
            if (!PasswordHelper.VerifyPassword(dto.OldPassword, user.HashPassword))
                throw new UnauthorizedAccessException("Senha atual incorreta.");

            user.HashPassword = PasswordHelper.HashPassword(dto.NewPassword);
            await _unitOfWork.UserRepository.UpdateAsync(user);
            await _unitOfWork.SaveAsync();

            return new UserDTO
            {
                UserId = user.UserId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                DocumentNumber = user.DocumentNumber,
                BirthDate = user.BirthDate,
                Phone = user.Phone,
                Role = user.Role.ToString(),
                IsActive = user.IsActive,
                HashPassword = user.HashPassword
            };
        }, "atualização de senha do usuário");
    }
    
    public async Task<UserDTO> ForgotPasswordAsync(int id, string newPassword)
    {
        return await ExceptionHandler.ExecuteWithHandling(async () =>
        {
            ValidatePassword(newPassword);
            var user = await _unitOfWork.UserRepository.GetByIdAsync(id);
            if (user == null)
                throw new KeyNotFoundException("Usuário não encontrado para recuperação de senha.");

            user.HashPassword = PasswordHelper.HashPassword(newPassword);
            await _unitOfWork.UserRepository.UpdateAsync(user);
            await _unitOfWork.SaveAsync();

            return new UserDTO
            {
                UserId = user.UserId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                DocumentNumber = user.DocumentNumber,
                BirthDate = user.BirthDate,
                Phone = user.Phone,
                Role = user.Role.ToString(),
                IsActive = user.IsActive,
                HashPassword = user.HashPassword
            };
        }, "recuperação de senha do usuário");
    }
}
