namespace Schedora.Domain.Services;

public interface IStorageService
{
    public Task UploadMedia(Stream media, string fileName, long userId);
    public Task<string> GetUrlMedia(string fileName, long userId);
}