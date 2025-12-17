using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Schedora.Domain.Interfaces;

namespace Schedora.Domain.DomainServices;

public interface IMediaDomainService
{
    public Task<DateTime?> GetTimeToMediaRetentEnds(long userId, Subscription subscription);
}

public class MediaDomainService : IMediaDomainService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<MediaDomainService> _logger;

    public MediaDomainService(IUnitOfWork uow, ILogger<MediaDomainService> logger)
    {
        _uow = uow;
        _logger = logger;
    }
    
    public async Task<DateTime?> GetTimeToMediaRetentEnds(long userId, Subscription subscription)
    {
        var firstMedia = await _uow.MediaRepository.GetFirstUserMedia(userId);
        
        var user = await _uow.GenericRepository.GetById<User>(userId);

        if (firstMedia is null)
            return null;

        var timeRetent = DateTime.UtcNow.AddTicks(subscription.TotalTimeToStorageRetention().Ticks);

        return timeRetent;
    }
}