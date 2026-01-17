namespace Schedora.Domain.Interfaces;

public interface IMediaRepository
{
    public Task<long> GetTotalUserMediaStoraged(long userId);
    public Task<Media?> GetFirstUserMedia(long userId);
    public Task<List<Media>> GetMediasByIds(List<long> mediaIds, long userId);
    public Task<MediaFolder?> GetMediaFolderByIdAndUser(long userId, long folderId);
}