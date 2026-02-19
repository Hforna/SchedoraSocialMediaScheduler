using System.Text.Json.Serialization;

public class TwitterUserDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("location")]
    public string Location { get; set; }

    [JsonPropertyName("profile_image_url")]
    public string ProfileImageUrl { get; set; }

    [JsonPropertyName("public_metrics")]
    public TwitterPublicMetricsDto PublicMetrics { get; set; }
}

public class TwitterPublicMetricsDto
{
    [JsonPropertyName("followers_count")]
    public int FollowersCount { get; set; }

    [JsonPropertyName("following_count")]
    public int FollowingCount { get; set; }

    [JsonPropertyName("tweet_count")]
    public int TweetCount { get; set; }

    [JsonPropertyName("listed_count")]
    public int ListedCount { get; set; }

    [JsonPropertyName("like_count")]
    public int LikeCount { get; set; }

    [JsonPropertyName("media_count")]
    public int MediaCount { get; set; }
}

public class TwitterUserInfosData
{
    [JsonPropertyName("data")]
    public TwitterUserDto Data { get; set; }
}