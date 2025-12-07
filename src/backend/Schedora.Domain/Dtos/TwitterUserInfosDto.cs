namespace Schedora.Domain.Dtos;

public class TwitterUserDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Username { get; set; }
    public DateTime Created_At { get; set; }
    public string Description { get; set; }
    public string Location { get; set; }
    public string Profile_Image_Url { get; set; }
    public TwitterPublicMetricsDto Public_Metrics { get; set; }
}

public class TwitterPublicMetricsDto
{
    public int Followers_Count { get; set; }
    public int Following_Count { get; set; }
    public int Tweet_Count { get; set; }
    public int Listed_Count { get; set; }
}

public class TwitterUserInfosData
{
    public TwitterUserDto Data { get; set;  }
}

