using System.ComponentModel.DataAnnotations;
using Viagium.EntitiesDTO;
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
    
    public async Task<User> AddAync(User user)
    {
        return await ExceptionHandler.ExecuteWithHandling(async () =>
        {
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
    

    private void ValidadeCustomRules(User user)
    {
        var errors = new List<string>();

        if (user.BirthDate > DateTime.Now)
            errors.Add("Data de nascimento não pode ser futura.");

        if (errors.Any())
            throw new ArgumentException(string.Join("\n", errors));
    }

    public async Task<User> LoginAsync(User user)
    {
        return await ExceptionHandler.ExecuteWithHandling(async () =>
        {
            var dbUser = await _unitOfWork.UserRepository.GetByEmailAsync(user.Email);
            if (dbUser == null)
                throw new ArgumentException("Usuário ou senha inválidos.");

            var hashedInputPassword = PasswordHelper.HashPassword(user.HashPassword);
            if (dbUser.HashPassword != hashedInputPassword)
                throw new ArgumentException("Usuário ou senha inválidos.");

            return dbUser;
        }, "login de usuário");
    }
}