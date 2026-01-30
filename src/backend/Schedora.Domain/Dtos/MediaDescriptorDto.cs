namespace Schedora.Domain.Dtos;

public class MediaDescriptorDto
{
    public MediaDescriptorDto(MediaType type, long sizeInMb, string format, int? durationSeconds, int width, int height)
    {
        Type = type;
        SizeInMb = sizeInMb;
        Format = format;
        DurationSeconds = durationSeconds;
        Width = width;
        Height = height;
    }

    public MediaType Type;
    public long SizeInMb;
    public string Format;
    public int? DurationSeconds;
    public int Width;
    public int Height;
}