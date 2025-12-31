namespace Schedora.Application;

public interface IMediaService
{
    public Task<(string, bool)> IsMediaTypeValid(Stream media);
}

public class MediaService : IMediaService
{
    public Task<(string, bool)> IsMediaTypeValid(Stream media)
    {
        throw new NotImplementedException();
    }
}