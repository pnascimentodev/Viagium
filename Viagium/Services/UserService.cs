using System.ComponentModel.DataAnnotations;
using Viagium.EntitiesDTO;
using Viagium.EntitiesDTO.Auth;
using Viagium.Models;
using Viagium.Repository;
using Viagium.Services.Auth;
using Viagium.Services.Interfaces;
using User = Viagium.Models.User;

namespace Viagium.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;
    
    public UserService(IUnitOfWork unitOfWork, IAuthService authService)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
    }

    public async Task<User> AddAync(User user, string password)
    {
        return await ExceptionHandler.ExecuteWithHandling(async () =>
        {
            // Verifica se já existe usuário com o mesmo e-mail
            if (await _unitOfWork.UserRepository.EmailExistsAsync(user.Email))
                throw new ArgumentException("Já existe um usuário cadastrado com este e-mail.");

            // Validação da senha antes do hash
            ValidatePassword(password);

            // Verifica se já existe usuário com o mesmo número de documento
            if (await _unitOfWork.UserRepository.DocumentNumberExistsAsync(user.DocumentNumber))
                throw new ArgumentException("Já existe um usuário cadastrado com este número de documento.");

            // Aplica o hash da senha
            user.HashPassword = Viagium.Services.PasswordHelper.HashPassword(password);

            //validação se o usuario esta conforme o contrato do DTO
            var validationContext = new ValidationContext(user);
            Validator.ValidateObject(user, validationContext, validateAllProperties: true);

            // Validações customizadas específicas do negócio
            ValidadeCustomRules(user);

            // usando o UnitOfWork para adicionar o usuário
            await _unitOfWork.UserRepository.AddAsync(user);
            await _unitOfWork.SaveAsync();
            
            return user;
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

    public async Task<User?> GetByIdAsync(int id)
    {
        return await ExceptionHandler.ExecuteWithHandling(async () =>
        {
            return await _unitOfWork.UserRepository.GetByIdAsync(id);
        }, "buscar usuário por id");
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await ExceptionHandler.ExecuteWithHandling(async () =>
        {
            return await _unitOfWork.UserRepository.GetAllAsync();
        }, "buscar todos usuários");
    }

    public async Task UpdateAsync(User user, string password)
    {
        await ExceptionHandler.ExecuteWithHandling(async () =>
        {
            // Verifica se já existe outro usuário com o mesmo e-mail
            if (await _unitOfWork.UserRepository.EmailExistsAsync(user.Email, user.UserId))
                throw new ArgumentException("Já existe outro usuário cadastrado com este e-mail.");

            // Validação da senha antes do hash
            ValidatePassword(password);

            // Aplica o hash da senha
            user.HashPassword = Viagium.Services.PasswordHelper.HashPassword(password);

            // Validação do usuário
            var validationContext = new ValidationContext(user);
            Validator.ValidateObject(user, validationContext, validateAllProperties: true);
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

    public async Task<User> ActivateAsync(int id)
    {
        return await ExceptionHandler.ExecuteWithHandling(async () =>
        {
            var user = await _unitOfWork.UserRepository.ActivateAsync(id);
            if (user == null)
                throw new KeyNotFoundException("Usuário não encontrado para ativação.");

            return user;
        }, "ativação de usuário");
    }

    private void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            throw new ArgumentException("A senha precisa possuir pelo menos 8 caracteres.");
    }
}

