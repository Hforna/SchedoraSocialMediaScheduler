namespace Schedora.Domain.Interfaces;

public interface ISocialAccountRepository
{
    public Task<bool> SocialAccountLinkedToUserExists(long userId, string platformUserId, string platform);
    public Task<List<SocialAccount>> GetUserSocialAccounts(long userId, string platform);
    public Task<Dictionary<string, int>> GetUserSocialAccountConnectedPerPlatform(long userId);
    public Task<List<SocialAccount>> GetAllTwitterSocialAccounts();
    public Task<List<SocialAccount>> GetAllUserSocialAccounts(long userId);
}