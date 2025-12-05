using System.Text.Json.Serialization;

namespace Schedora.Domain.Dtos;

public class LinkedInUserInfosDto
{
    [JsonPropertyName("sub")]
    public string Id {  get; set; }
    [JsonPropertyName("name")]
    public string UserName { get; set; }
    [JsonPropertyName("email")]
    public string Email { get; set; }
    [JsonPropertyName("given_name")]
    public string GivenName { get; set; }
    [JsonPropertyName("family_name")]
    public string FamilyName { get; set; }
    [JsonPropertyName("picture")]
    public string ProfilePicture { get; set; }
}