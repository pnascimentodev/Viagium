using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Viagium.EntitiesDTO;
using Viagium.EntitiesDTO.Auth;
using Viagium.Models;
using Viagium.Repository;
using Viagium.Repository.Interface;
using Viagium.Services.Auth;
using Viagium.Services.Interfaces;

namespace Viagium.Services;

public class AdminService : IAdminService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;
    private readonly IMapper _mapper;

    public AdminService(IUnitOfWork unitOfWork, IAuthService authService, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
        _mapper = mapper;
    }

    public async Task<User> RegisterAsync(AdminRegisterDTO adminRegisterDto)
    {
        var user = _mapper.Map<User>(adminRegisterDto);
        user.Role = adminRegisterDto.Role;
        user.IsActive = true;
        // Valida��o de senha
        ValidatePassword(adminRegisterDto.Password);
        // Valida��o de unicidade
        if (await _unitOfWork.UserRepository.EmailExistsAsync(user.Email))
            throw new ArgumentException("J� existe um usu�rio cadastrado com este e-mail.");
        if (await _unitOfWork.UserRepository.DocumentNumberExistsAsync(user.DocumentNumber))
            throw new ArgumentException("J� existe um usu�rio cadastrado com este n�mero de documento.");
        // Hash da senha
        user.HashPassword = PasswordHelper.HashPassword(adminRegisterDto.Password);
        // Valida��o de DataAnnotations
        Validator.ValidateObject(user, new ValidationContext(user), true);
        await _unitOfWork.UserRepository.AddAsync(user);
        await _unitOfWork.SaveAsync();
        return user;
    }

    public async Task<LoginResponseDTO> LoginWithRoleAsync(LoginRequestDTO loginRequest)
    {
        // Permite login apenas para Admin e Suporte
        var user = await _unitOfWork.UserRepository.GetByEmailAsync(loginRequest.Email);
        if (user == null || !user.IsActive || user.DeletedAt != null)
            throw new UnauthorizedAccessException("Usu�rio ou senha inv�lidos.");
        if (!PasswordHelper.VerifyPassword(loginRequest.Password, user.HashPassword))
            throw new UnauthorizedAccessException("Usu�rio ou senha inv�lidos.");
        if (user.Role != Role.Admin && user.Role != Role.Support)
            throw new UnauthorizedAccessException("Acesso n�o permitido para este tipo de usu�rio.");
        var token = _authService.LoginWithRoleAsync(loginRequest, user.Role).Result.Token;
        return new LoginResponseDTO
        {
            Id = user.UserId.ToString(),
            Role = user.Role.ToString(),
            Token = token
        };
    }

    public async Task<AdminDTO?> GetByIdAsync(int id)
    {
        var user = await _unitOfWork.UserRepository.GetByIdAsync(id);
        if (user == null || user.Role != Role.Admin )
            return null;
        return _mapper.Map<AdminDTO>(user);
    }

    public async Task<List<AdminDTO>> GetAllAsync()
    {
        var users = await _unitOfWork.UserRepository.GetAllAsync();
        var admins = users.Where(u => u.Role == Role.Admin).ToList();
        return admins.Select(u => _mapper.Map<AdminDTO>(u)).ToList();
    }

    public async Task<List<AdminDTO>> GetAllActiveAsync()
    {
        var users = await _unitOfWork.UserRepository.GetAllAsync();
        var admins = users.Where(u => (u.Role == Role.Admin) && u.IsActive && u.DeletedAt == null).ToList();
        return admins.Select(u => _mapper.Map<AdminDTO>(u)).ToList();
    }

    public async Task<AdminDTO> UpdateAsync(int id, AdminUpdateDTO adminUpdateDto)
    {
        var user = await _unitOfWork.UserRepository.GetByIdAsync(id);
        if (user == null || (user.Role != Role.Admin && user.Role != Role.Support))
            throw new KeyNotFoundException("Admin n�o encontrado.");
        _mapper.Map(adminUpdateDto, user);
        user.Role = user.Role; // Mant�m a role
        user.UpdatedAt = DateTime.Now;
        ValidatePassword(adminUpdateDto.Password);
        user.HashPassword = PasswordHelper.HashPassword(adminUpdateDto.Password);
        Validator.ValidateObject(user, new ValidationContext(user), true);
        await _unitOfWork.UserRepository.UpdateAsync(user);
        await _unitOfWork.SaveAsync();
        return _mapper.Map<AdminDTO>(user);
    }

    public async Task<User> DesativateUserAsync(int id)
    {
        return await ExceptionHandler.ExecuteWithHandling(async () =>
    {
        var user = await _unitOfWork.UserRepository.DesativateAsync(id);
        if (user == null)
            throw new KeyNotFoundException("Usu�rio n�o encontrado para desativa��o.");
        return user;
    }, "desativa��o de usu�rio");
       
    }

    public async Task<User> ActivateUserAsync(int id)
    {
        return await ExceptionHandler.ExecuteWithHandling(async () =>
        {
            var user = await _unitOfWork.UserRepository.ActivateAsync(id);
            if (user == null)
                throw new KeyNotFoundException("Usu�rio n�o encontrado para ativa��o.");
            return user;
        }, "ativa��o de usu�rio");
       
    }

    private void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            throw new ArgumentException("A senha precisa possuir pelo menos 8 caracteres.");
    }
}
