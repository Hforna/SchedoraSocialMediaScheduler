using System.Net.Mime;
using FileTypeChecker.Extensions;
using FileTypeChecker.Types;
using Schedora.Application.Services;
using Schedora.Domain.Dtos;
using SixLabors.ImageSharp;
using Xabe.FFmpeg;

namespace Schedora.Application;

public interface IMediaHandlerService
{
    public long GetMegaBytesSizeFromFile(Stream file);
    public Task<int> GetTotalVideoDuration(Stream video);
    public string GetExtension(string fileName);
    public Task<MediaType> GetType(Stream media);
    public Task<bool> IsValid(Stream media);
    public Task<(int width, int height)> GetDimensions(Stream media, MediaType type);
}

public class MediaHandlerService : IMediaHandlerService
{
    private readonly IVideoProcessor _videoProcessor;
    private readonly IImageProcessor _imageProcessor;

    public MediaHandlerService(IVideoProcessor videoProcessor, IImageProcessor imageProcessor)
    {
        _videoProcessor = videoProcessor;
        _imageProcessor = imageProcessor;
    }

    public long GetMegaBytesSizeFromFile(Stream file) => file.Length / 1000000;
    public async Task<int> GetTotalVideoDuration(Stream video)
    {
        return 2;
    }

    public async Task<(int width, int height)> GetDimensions(Stream media, MediaType type)
    {
        int height = 0, width = 0;

        if (type == MediaType.VIDEO)
        {
            var videoInfos = await _videoProcessor.GetVideoInfos(media);
            width = videoInfos.Width;
            height = videoInfos.Height;
        }
        if (type == MediaType.IMAGE)
            (height, width) = await _imageProcessor.GetDimensions(media);
            
        return (width, height);
    }

    public string GetExtension(string fileName) => Path.GetExtension(fileName);

    public async Task<MediaType> GetType(Stream media)
    {
        var validateImage = _imageProcessor.IsValid(media);
        
        if (validateImage)
            return MediaType.IMAGE;
        if(await _videoProcessor.IsVideoValid(media))
            return MediaType.VIDEO;
        
        throw new DomainException("Media type not supported");
    }

    public async Task<bool> IsValid(Stream media)
    {
        var validateImage = _imageProcessor.IsValid(media);
        
        return validateImage || await _videoProcessor.IsVideoValid(media);
    }
}

public abstract class MediaBaseProcessor
{
    public void ResetStream(Stream media)
    {
        if(media.CanSeek)
            media.Position = 0;
    }
}

public interface IVideoProcessor
{
    public Task<bool> IsVideoValid(Stream media);
    public Task<VideoInfosDto> GetVideoInfos(Stream media);
}

public class FFmpegProcessor : MediaBaseProcessor, IVideoProcessor
{
    private async Task<(FileStream file, string tempPath)> CreateTempFile(Stream file)
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.mp4");
        var tempFile = File.Create(tempPath);
        
        await file.CopyToAsync(tempFile);
        
        return (tempFile, tempPath);
    }
    
    public async Task<bool> IsVideoValid(Stream media)
    {
        var infos = await GetProcessorMediaInfos(media);

        return infos.VideoStreams.Any();
    }

    public async Task<VideoInfosDto> GetVideoInfos(Stream media)
    {
        var infos = await GetProcessorMediaInfos(media);
        
        var videoStreams = infos.VideoStreams.FirstOrDefault();
            
        if(videoStreams is null)
            throw new InternalServiceException("There has occurred and error while trying to get media infos");
            
        var videoInfos = new VideoInfosDto()
        {
            Duration = infos.Duration.Seconds,
            Height = videoStreams.Height,
            Width  = videoStreams.Width
        };

        return videoInfos;
    }

    private async Task<IMediaInfo> GetProcessorMediaInfos(Stream media)
    {
        var (file, filePath) = await CreateTempFile(media);

        try
        {
            var infos = await Xabe.FFmpeg.FFmpeg.GetMediaInfo(filePath);

            return infos;
        }
        catch(Exception e)
        {
            throw new InternalServiceException("It was not possible to get media infos from file");
        }
        finally
        {
            ResetStream(media);
            await file.DisposeAsync();
        }
    }
}

public interface IImageProcessor
{
    public bool IsValid(Stream file);
    public Task<(int width, int height)> GetDimensions(Stream media);
}

public class ImageProcessor : MediaBaseProcessor, IImageProcessor
{
    public bool IsValid(Stream media)
    {
        bool isImage = false;

        if (media.Is<JointPhotographicExpertsGroup>())
            isImage = true;
        if (media.Is<PortableNetworkGraphic>())
            isImage = true;

        ResetStream(media);
        
        return isImage;
    }

    public async Task<(int width, int height)> GetDimensions(Stream media)
    {
        try
        {
            var imageInfos = await Image.LoadAsync(media);
            
            return (imageInfos.Width, imageInfos.Height);
        }
        catch (Exception e)
        {
            throw new InternalServiceException("It was not possible process media of type image");
        }
    }
}
