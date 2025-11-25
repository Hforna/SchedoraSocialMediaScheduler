using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Schedora.Application.Requests;
using Schedora.Application.Responses;
using Schedora.Application.Utils;
using Schedora.Domain.Entities;
using Schedora.Domain.Exceptions;
using Schedora.Domain.Interfaces;
using Schedora.Domain.Services;
using Schedora.Domain.ValueObjects;

namespace Schedora.Application.Services;

public interface IAuthService
{
    public Task<UserResponse> RegisterUser(UserRegisterRequest request, string uri);
}

public class AuthService : IAuthService
{
    public AuthService(ILogger<IAuthService> logger, IMapper mapper, 
        ITokenService tokenService, IUnitOfWork uow, 
        IPasswordCryptography cryptography, IEmailService emailService, UserManager<User> userManager)
    {
        _logger = logger;
        _mapper = mapper;
        _tokenService = tokenService;
        _uow = uow;
        _emailService = emailService;
        _cryptography = cryptography;
        _userManager = userManager;
    }

    private readonly ILogger<IAuthService> _logger;
    private readonly IMapper _mapper;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _uow;
    private readonly IPasswordCryptography _cryptography;
    private readonly IEmailService _emailService;
    private readonly UserManager<User> _userManager;
    
    public async Task<UserResponse> RegisterUser(UserRegisterRequest request, string uri)
    {
        if (!Email.IsValidEmail(request.Email))
            throw new RequestException("E-mail format is not valid");
        
        var userByEmailExists = await _uow.UserRepository.UserByEmailExists(request.Email);

        if (userByEmailExists)
            throw new ConflictException("User with this e-mail already exists");

        var user = _mapper.Map<User>(request);
        user.PasswordHash = _cryptography.HashPassword(request.Password);

        await _uow.GenericRepository.Add<User>(user);
        await _uow.Commit();

        var tokenConfirmation = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var uriWithToken = $"{uri}?email={user.Email}&token={tokenConfirmation}";
        
        var emailMessage = await _emailService.RenderEmailConfirmation(
            user.UserName!, 
            uri, 
            CompanyConstraints.CompanyName, 
            24);
        await _emailService.SendEmail(user.Email, user.UserName, emailMessage, $"Hi {user.UserName} confirm your e-mail here");

        return _mapper.Map<UserResponse>(user);
    }
}