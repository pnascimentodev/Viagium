using System.ComponentModel.DataAnnotations;
using Viagium.EntitiesDTO;
using Viagium.EntitiesDTO.User;
using Viagium.Models;
using Viagium.Repository;
using User = Viagium.Models.User;

namespace Viagium.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public UserService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
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

            var user = new User
            {
                Email = userCreateDto.Email,
                FirstName = userCreateDto.FirstName,
                LastName = userCreateDto.LastName,
                DocumentNumber = userCreateDto.DocumentNumber,
                BirthDate = userCreateDto.BirthDate,
                HashPassword = PasswordHelper.HashPassword(password)
            };

            //validação se o usuario esta conforme o contrato do DTO
            var validationContext = new ValidationContext(userCreateDto);
            Validator.ValidateObject(userCreateDto, validationContext, validateAllProperties: true);

            // Validações customizadas específicas do negócio
            ValidadeCustomRules(user);

            // usando o UnitOfWork para adicionar o usuário
            await _unitOfWork.UserRepository.AddAsync(user);
            await _unitOfWork.SaveAsync();

            // Mapeamento simples para UserDTO
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
        }, "criação de usuário");
        
        
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

}
