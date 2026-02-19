using System.Text.Json.Serialization;

namespace Schedora.Domain.Dtos.Externals;

public class TwitterMediaUploadDto
{
    [JsonPropertyName("data")]
    public TwitterMediaUploadDataDto? Data { get; set; }

    [JsonPropertyName("errors")]
    public List<TwitterMediaUploadErrorDto>? Errors { get; set; }
}

public class TwitterMediaUploadDataDto
{
    [JsonPropertyName("expires_after_secs")]
    public int ExpiresAfterSecs { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("media_key")]
    public string? MediaKey { get; set; }

    [JsonPropertyName("processing_info")]
    public TwitterMediaProcessingInfoDto? ProcessingInfo { get; set; }

    [JsonPropertyName("size")]
    public int Size { get; set; }
}

public class TwitterMediaProcessingInfoDto
{
    [JsonPropertyName("check_after_secs")]
    public int CheckAfterSecs { get; set; }

    [JsonPropertyName("progress_percent")]
    public int ProgressPercent { get; set; }

    [JsonPropertyName("state")]
    public string? State { get; set; }
}

public class TwitterMediaUploadErrorDto
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("detail")]
    public string? Detail { get; set; }

    [JsonPropertyName("status")]
    public int Status { get; set; }
}