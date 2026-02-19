namespace Schedora.Domain.Dtos;

public class MediaUploadDataDto
{
    public int ExpiresAfterSecs { get; set; }

    public string? Id { get; set; }

    public string? MediaKey { get; set; }

    public MediaProcessingInfoDto? ProcessingInfo { get; set; }

    public int Size { get; set; }
}

public class MediaProcessingInfoDto
{
    public int CheckAfterSecs { get; set; }

    public int ProgressPercent { get; set; }

    public string? State { get; set; }
}