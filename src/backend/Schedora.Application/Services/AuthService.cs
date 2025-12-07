using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System.Text;
using SocialScheduler.Domain.Constants;

namespace Schedora.Application.Services;

public interface IAuthService
{
    public Task<UserResponse> RegisterUser(UserRegisterRequest request, string emailConfirmatioUri);
    public Task ConfirmEmail(string email, string token);
    public Task<LoginResponse> LoginByApplication(LoginRequest request);
    public Task<LoginResponse> RefreshToken(string refreshToken);
    public Task ResetPasswordRequest(string email, string uri);
    public Task ResetUserPassword(string email, string token, string password);
    public Task RevokeToken();
}

public class AuthService : IAuthService
{
    public AuthService(ILogger<IAuthService> logger, IMapper mapper, 
        ITokenService tokenService, IUnitOfWork uow, 
        IPasswordCryptographyService cryptographyService, IEmailService emailService, 
        UserManager<User> userManager, IActivityLogService activityLogService)
    {
        _logger = logger;
        _activityLogService = activityLogService;
        _mapper = mapper;
        _tokenService = tokenService;
        _uow = uow;
        _emailService = emailService;
        _cryptographyService = cryptographyService;
        _userManager = userManager;
    }

    private readonly ILogger<IAuthService> _logger;
    private readonly IMapper _mapper;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _uow;
    private readonly IPasswordCryptographyService _cryptographyService;
    private readonly IEmailService _emailService;
    private readonly UserManager<User> _userManager;
    private readonly IActivityLogService _activityLogService;
    
    public async Task<UserResponse> RegisterUser(UserRegisterRequest request, string emailConfirmatioUri)
    {
        if (!Email.IsValidEmail(request.Email))
            throw new RequestException("E-mail format is not valid");
        
        var userByEmailExists = await _uow.UserRepository.UserByEmailExists(request.Email);

        if (userByEmailExists)
            throw new ConflictException("User with this e-mail already exists");

        var user = _mapper.Map<User>(request);
        user.PasswordHash = _cryptographyService.HashPassword(request.Password);
        user.SecurityStamp = Guid.NewGuid().ToString();

        var usersNotActive = await _uow.UserRepository.GetUsersNotActiveByEmail(request.Email);
        if(usersNotActive.Any())
            _uow.GenericRepository.DeleteRange<User>(usersNotActive);
        await _uow.GenericRepository.Add<User>(user);
        await _uow.Commit();

        var tokenConfirmation = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(tokenConfirmation));
        var uriWithToken = $"{emailConfirmatioUri}?email={user.Email}&token={encodedToken}";
        _logger.LogInformation("Uri with token generated {uri}", uriWithToken);
        
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
        var user = await _uow.UserRepository.UserByEmailNotConfirmed(email)
            ?? throw new RequestException("User with this e-mail has not been found");

        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        var validateEmail = await _userManager.ConfirmEmailAsync(user, decodedToken);

        if (!validateEmail.Succeeded)
            throw new UnauthorizedException(string.Join(", ", validateEmail.Errors.ToList()));

        user.IsActive = true;
        user.EmailConfirmed = true;

        var logDetails = new
        {
            Email = user.Email,
            Token = decodedToken
        };
        await _activityLogService.LogAsync(user.Id, ActivityActions.EMAIL_VERIFIED, "user", user.Id, logDetails, false);

        _uow.GenericRepository.Update<User>(user);
        await _uow.Commit();
    }

    public async Task<LoginResponse> LoginByApplication(LoginRequest request)
    {
        var user = await _uow.UserRepository.UserByEmail(request.Email)
            ?? throw new NotFoundException("User was not found");

        (var token, DateTime expiration) = _tokenService.GenerateToken(user.Id, user.UserName!);

        var refreshToken = _tokenService.GenerateRefreshToken();
        var refreshTokenExpiration = _tokenService.GenerateRefreshTokenExpiration();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpires = refreshTokenExpiration;

        _uow.GenericRepository.Update<User>(user);
        await _uow.Commit();

        await _activityLogService.LogAsync(user.Id, ActivityActions.USER_LOGIN, "user", user.Id);

        var response = new LoginResponse()
        {
            RefreshToken = refreshToken,
            AccessExpiresAt = expiration,
            AccessToken = token,
            RefreshExpiresAt = refreshTokenExpiration,
        };

        return response;
    }

    public async Task<LoginResponse> RefreshToken(string refreshToken)
    {
        var user = await _tokenService.GetUserByToken();
        var userRefreshToken = user.RefreshToken;

        if (userRefreshToken is null || refreshToken != userRefreshToken)
            throw new RequestException("Refresh token invalido");

        user.RefreshToken = _tokenService.GenerateRefreshToken();
        user.RefreshTokenExpires = _tokenService.GenerateRefreshTokenExpiration();

        _uow.GenericRepository.Update<User>(user);
        await _uow.Commit();

        _logger.LogInformation("New refresh token generated for user: {user.Id}", user.Id);

        var claims = _tokenService.GetTokenClaims();

        var accessToken = _tokenService.GenerateToken(user.Id, user.UserName, claims);

        return new LoginResponse()
        {
            AccessToken = accessToken.token,
            RefreshToken = user.RefreshToken,
            RefreshExpiresAt = (DateTime)user.RefreshTokenExpires,
            AccessExpiresAt = accessToken.expiresAt
        };
    }

    public async Task ResetPasswordRequest(string email, string uri)
    {
        var user = await _uow.UserRepository.UserByEmail(email)
            ?? throw new NotFoundException("User by e-mail was not found");

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        var resetUri = $"{uri}?email={user.Email}&token={encodedToken}";
        var emailTemplate = await _emailService.RenderResetPassword(user.UserName, CompanyConstraints.CompanyName, resetUri, 24);
        await _emailService.SendEmail(user.Email, user.UserName, emailTemplate, $"Hi {user.UserName} reset your password here");
    }

    public async Task ResetUserPassword(string email, string token, string password)
    {
        var user = await _uow.UserRepository.UserByEmail(email)
            ?? throw new NotFoundException("User by e-mail was not found");

        
    }

    public async Task RevokeToken()
    {
        var user = await _tokenService.GetUserByToken();

        user!.RefreshToken = null;
        user.RefreshTokenExpires = null;
        
        _uow.GenericRepository.Update<User>(user);
        await _uow.Commit();
    }
}