using SocialScheduler.Domain.Constants;

namespace Schedora.Application.Services;

public interface IUserService
{
    public Task<UserResponse> GetUserAuthenticatedInfos();
    public Task UpdatePassword(UpdatePasswordRequest request);
    public Task<UserResponse> UpdateUserInfos(UpdateUserRequest request);
    public Task<string> GetUserSubscription();
}

public class UserService : IUserService
{
    public UserService(ITokenService tokenService, IMapper mapper, ILogger<IUserService> logger, IUnitOfWork uow)
    {
        _tokenService = tokenService;
        _mapper = mapper;
        _logger = logger;
        _uow = uow;
    }

    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;
    private readonly ILogger<IUserService> _logger;
    private readonly IUnitOfWork _uow;
    private readonly IPasswordCryptography _passwordCryptography;

    public async Task<UserResponse> GetUserAuthenticatedInfos()
    {
        var user = await _tokenService.GetUserByToken();

        return _mapper.Map<UserResponse>(user);
    }

    public async Task UpdatePassword(UpdatePasswordRequest request)
    {
        var user = await _tokenService.GetUserByToken();

        var isPasswordValid = _passwordCryptography.ValidateHash(request.OldPassword, user.PasswordHash!);
        if (!isPasswordValid)
            throw new UnauthorizedAccessException("Invalid old password");

        var hashPassword = _passwordCryptography.HashPassword(request.NewPassword);

        user.UpdatePassword(request.NewPassword, hashPassword);

        _uow.GenericRepository.Update<User>(user);
        await _uow.Commit();
    }

    public async Task<UserResponse> UpdateUserInfos(UpdateUserRequest request)
    {
        var user = await _tokenService.GetUserByToken();

        _mapper.Map(request, user);

        _uow.GenericRepository.Update<User>(user!);
        await _uow.Commit();

        return _mapper.Map<UserResponse>(user);
    }

    public async Task<string> GetUserSubscription()
    {
        var user = await _tokenService.GetUserByToken();

        return user!.SubscriptionTier.ToString();
    }
}