using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Schedora.Application.Requests;
using Schedora.Application.Responses;
using Schedora.Application.Utils;
using Schedora.Domain.Entities;
using Schedora.Domain.Exceptions;
using Schedora.Domain.Interfaces;
using Schedora.Domain.Services;
using Schedora.Domain.ValueObjects;
using System.Text;

namespace Schedora.Application.Services;

public interface IAuthService
{
    public Task<UserResponse> RegisterUser(UserRegisterRequest request, string uri);
    public Task ConfirmEmail(string email, string token);
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
        user.SecurityStamp = Guid.NewGuid().ToString();

        var usersNotActive = await _uow.UserRepository.GetUsersNotActiveByEmail(request.Email);
        if(usersNotActive.Any())
            _uow.GenericRepository.DeleteRange<User>(usersNotActive);
        await _uow.GenericRepository.Add<User>(user);
        await _uow.Commit();

        var tokenConfirmation = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(tokenConfirmation));
        var uriWithToken = $"{uri}?email={user.Email}&token={encodedToken}";
        
        var emailMessage = await _emailService.RenderEmailConfirmation(
            user.UserName!, 
            uriWithToken, 
            CompanyConstraints.CompanyName, 
            24);
        await _emailService.SendEmail(user.Email, user.UserName, emailMessage, $"Hi {user.UserName} confirm your e-mail here");

        return _mapper.Map<UserResponse>(user);
    }

    public async Task ConfirmEmail(string email, string token)
    {
        var user = await _uow.UserRepository.UserByEmail(email)
            ?? throw new RequestException("User with this e-mail has not been found");

        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        var validateEmail = await _userManager.ConfirmEmailAsync(user, decodedToken);

        if (!validateEmail.Succeeded)
            throw new UnauthorizedException(string.Join(", ", validateEmail.Errors.ToList()));

        user.IsActive = true;
        user.EmailConfirmed = true;

        _uow.GenericRepository.Update<User>(user);
        await _uow.Commit();
    }
}