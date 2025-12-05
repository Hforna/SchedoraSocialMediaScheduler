namespace Schedora.Domain.Dtos;

public class SocialAccountConnectedDto
{
    public SocialAccountConnectedDto(long socialAccountId, long userId)
    {
        SocialAccountId = socialAccountId;
        UserId = userId;
    }

    public long SocialAccountId  { get; set; }
    public long UserId  { get; set; }
}