namespace Schedora.Domain.Services;

public interface ISocialPostService
{
    public string Platform { get; }
    public Task CreatePost(Schedora.Domain.Dtos.SocialCreatePostRequestDto request, Schedora.Domain.Entities.SocialAccount userSocialAccount, CancellationToken cancellationToken = default);
}