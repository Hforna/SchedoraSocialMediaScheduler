using Schedora.Domain.Dtos;

namespace Schedora.Domain.Services;

public interface IOAuthTokenService
{
    public string Platform { get; }
    public Task<ExternalServicesTokensDto> RefreshToken(string refreshToken);
}