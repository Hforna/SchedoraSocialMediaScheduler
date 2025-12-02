namespace Schedora.Application.Services;

public interface ISocialAccountService
{
    
}

public class SocialAccountService : ISocialAccountService
{
    private readonly ILogger<ISocialAccountService> _logger;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _uow;
}