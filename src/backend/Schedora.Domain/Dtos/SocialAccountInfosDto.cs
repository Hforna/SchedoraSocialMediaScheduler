using System.Text.Json.Serialization;

namespace Schedora.Domain.Dtos;

public class SocialAccountInfosDto
{
    public string UserId { get; set; }
    public string? Email { get; set; }
    public string FullName { get; set; }
    public string UserName { get; set; }
    public string? PictureUrl { get; set; }
    public int FollowersCount { get; set; }
    public int FollowingCount { get; set; }
}