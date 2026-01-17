namespace Schedora.Application.Responses;

public class MediaResponse
{
    public string FileName { get; private set; }
    public long FileSize { get; private set; }
    public int? Width { get; private set; }
    public int? Height { get; private set; }
    public int? Duration { get; private set; }
    public bool IsProcessed { get; private set; }
    public ProcessingStatus ProcessingStatus { get; private set; }
    public DateTime? UploadedAt { get; private set; }

}