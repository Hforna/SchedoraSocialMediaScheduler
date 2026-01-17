using Schedora.Domain.Exceptions;

namespace Schedora.Domain.Entities;

public class Subscription : IEntity
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public User User { get; set; }
    public string GatewayProvider { get; set; } = string.Empty;
    public string GatewaySubscriptionId { get; set; } = string.Empty;
    public string GatewayCustomerId { get; set; } = string.Empty;
    public string GatewayPriceId { get; set; } = string.Empty;
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;
    public SubscriptionEnum SubscriptionTier { get; set; }
    public DateTime? CurrentPeriodEndsAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CanceledAt { get; set; }

    public Subscription(long userId, SubscriptionEnum subscriptionTier, string? gatewayProvider,
        SubscriptionStatus status,
        string? gatewayCustomerId, string? gatewayPriceId, string? gatewaySubscriptionId, DateTime? currentPeriodEndsAt, DateTime createdAt)
    {
        UserId = userId;
        SubscriptionTier = subscriptionTier;
        GatewayProvider = gatewayProvider;
        GatewayCustomerId = gatewayCustomerId;
        GatewayPriceId = gatewayPriceId;
        GatewaySubscriptionId = gatewaySubscriptionId;
        Status = status;
        CurrentPeriodEndsAt = currentPeriodEndsAt;
        CreatedAt = createdAt;
    }

    public Subscription(long userId, SubscriptionEnum subscriptionTier)
    {
        UserId = userId;
        SubscriptionTier = subscriptionTier;
    }
    
    public void CancelSubscription()
    {
        if (SubscriptionTier == SubscriptionEnum.FREE)
            throw new DomainException("Cannot cancel a free subscription");
    
        if (Status == SubscriptionStatus.Canceled)
            throw new DomainException("Subscription is already canceled");
        
        GatewayPriceId = string.Empty;
        GatewaySubscriptionId = string.Empty;
        GatewayProvider = string.Empty;
        Status = SubscriptionStatus.Canceled;
        CanceledAt = DateTime.UtcNow;
        if (DateTime.UtcNow >= CurrentPeriodEndsAt)
        {
            Status = SubscriptionStatus.Active;
            SubscriptionTier = SubscriptionEnum.FREE;
        }
        CurrentPeriodEndsAt = null;
    }

    public bool CanSchedulePost(int totalSocialAccounts)
    {
        return SubscriptionTier switch
        {
            SubscriptionEnum.FREE => totalSocialAccounts <= 10,
            SubscriptionEnum.BUSINESS => true,
            SubscriptionEnum.PRO => true,
            _ => throw new DomainException($"Subscription {SubscriptionTier} is not supported.")
        };
    }

    public void ValidateMediaUploading(long fileSizeInMb, MediaType mediaType, int? videoMinutesDuration = null)
    {
        if(mediaType != MediaType.VIDEO && videoMinutesDuration != null)
            throw new DomainException($"Video duration must be null if media type is image");
        if (mediaType == MediaType.VIDEO && videoMinutesDuration == null)
            throw new InternalServiceException("Video duration must not be null if media type is video");

        if (mediaType == MediaType.IMAGE)
        {
            var maxSize = MaxImageSizeInMB();
            if (fileSizeInMb > maxSize)
                throw new DomainException($"Max file size for free subscription tier is {maxSize}mb");
        }else if (mediaType == MediaType.VIDEO)
        {
            if (SubscriptionTier == SubscriptionEnum.FREE)
                throw new DomainException("Video media type is not supported for the free subscription tier");
            
            var maxSize = MaxVideoSizeInMB();
            if(fileSizeInMb > maxSize)
                throw new DomainException($"Max video size is {maxSize}mb");

            var maxDuration = MaxVideoDurationInMinutes();
            if(videoMinutesDuration > maxDuration)
                throw new DomainException($"Video duration must be less than {maxDuration} minutes");
        }
    }

    public void ValidateStorageLimit(long fileSizeInMb, long totalStoraged)
    {
        var storageLimit = TotalStorageAllowed();
        if (fileSizeInMb + totalStoraged > storageLimit)
            throw new DomainException($"Storage remaining is {storageLimit - totalStoraged}mb");
    }
    
    private long MaxImageSizeInMB()
    {
        return GetRuleForEachPlan<long>(5, 10, 25);
    }

    private long MaxVideoSizeInMB()
    {
        return GetRuleForEachPlan<long>(0, 100, 500);
    }

    private int MaxVideoDurationInMinutes()
    {
        return GetRuleForEachPlan(0, 2, 10);
    }
    
    public int MaxAccountsPerPlatformBySubscription()
    {
        return GetRuleForEachPlan<int>(1, 3, 10);
    }

    //Return total storage to SubscriptionTier plan in megabytes
    public long TotalStorageAllowed()
    {
        return GetRuleForEachPlan<long>(500, 5000, 50000);
    }

    public DateTime GetRetentionEndDate(DateTime firstMedia)
    {
        var retentionLimit = StorageRetentionLimit();

        return firstMedia.AddTicks(retentionLimit.Ticks);
    }

    private TimeSpan StorageRetentionLimit()
    {
        return GetRuleForEachPlan<TimeSpan>(TimeSpan.FromDays(180), TimeSpan.FromDays(730), TimeSpan.FromDays(3000));
    }

    private T GetRuleForEachPlan<T>(T free, T pro, T business)
    {
        return SubscriptionTier switch
        {
            SubscriptionEnum.FREE => free,
            SubscriptionEnum.PRO => pro,
            SubscriptionEnum.BUSINESS => business,
            _ => throw new DomainException($"Subscription {SubscriptionTier} is not supported.")
        };
    }
}