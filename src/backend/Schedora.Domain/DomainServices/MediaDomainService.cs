using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Schedora.Domain.Exceptions;
using Schedora.Domain.Interfaces;

namespace Schedora.Domain.DomainServices;

public interface IMediaDomainService
{
    public Task<DateTime?> GetTimeToMediaRetentEnds(long userId, Subscription subscription);
    public Task ValidateUserUploadingMedia(User user, Subscription subscription, long fileSizeMb, MediaType mediaType, int? videoDuration = null);
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

        if (firstMedia.UploadedAt is null)
            throw new InternalServiceException("It is not validate time to retention ends when uploaded is null");
        
        var timeRetent = subscription.GetRetentionEndDate((DateTime)firstMedia.UploadedAt);

        return timeRetent;
    }

    public async Task ValidateUserUploadingMedia(User user, Subscription subscription, 
        long fileSizeMb, MediaType mediaType, int? videoDuration = null)
    {
        subscription.ValidateMediaUploading(fileSizeMb, mediaType, videoDuration);

        var totalStoraged = await _uow.StorageRepository.GetTotalStorageUsedByUser(user.Id);

        subscription.ValidateStorageLimit(fileSizeMb, totalStoraged);
    }
}