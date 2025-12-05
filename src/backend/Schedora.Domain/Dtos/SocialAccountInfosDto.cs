using System.Text.Json.Serialization;

namespace Schedora.Domain.Dtos;

public class SocialAccountInfosDto
{
    public string UserId { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string UserName { get; set; }
    public string ProfileImageUrl { get; set; }
    public int FollowerCount { get; set; }
    
}