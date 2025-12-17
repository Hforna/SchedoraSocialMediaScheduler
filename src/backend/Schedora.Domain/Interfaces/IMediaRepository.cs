namespace Schedora.Domain.Interfaces;

public interface IMediaRepository
{
    public Task<long> GetTotalUserMediaStoraged(long userId);
    public Task<Media?> GetFirstUserMedia(long userId);
}