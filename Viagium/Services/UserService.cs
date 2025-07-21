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

    public async Task UpdateAsync(User user)
    {
        await ExceptionHandler.ExecuteWithHandling(async () =>
        {
            // Validação do usuário
            var validationContext = new ValidationContext(user);
            Validator.ValidateObject(user, validationContext, validateAllProperties: true);
            ValidadeCustomRules(user);
            await _unitOfWork.UserRepository.UpdateAsync(user);
            await _unitOfWork.SaveAsync();
        }, "atualizar usuário");
    }

}