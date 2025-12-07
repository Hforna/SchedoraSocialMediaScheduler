namespace Schedora.Domain.Interfaces;

public interface ISocialAccountRepository
{
    public Task<bool> SocialAccountLinkedToUserExists(long userId, string platformUserId, string platform);
}