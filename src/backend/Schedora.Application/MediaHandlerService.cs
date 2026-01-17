using System.Net.Mime;
using FFMediaToolkit.Decoding;
using FFmpeg.AutoGen;
using FileTypeChecker.Extensions;
using FileTypeChecker.Types;
using SixLabors.ImageSharp;

namespace Schedora.Application;

public interface IMediaHandlerService
{
    public long GetMegaBytesSizeFromFile(Stream file);
    public Task<int> GetTotalVideoDuration(Stream video);
    public string GetMediaExtension(string fileName);
    public Task<MediaType> GetMediaType(Stream media);
    public Task<bool> IsMediaValid(Stream media);
    public Task<string> GetMediaFullName(Stream media);
    public (bool isImage, string ext) ValidateImage(Stream file);
    public Task<(int width, int height)> GetMediaDimensions(Stream media, MediaType type);
}

public class MediaHandlerService : IMediaHandlerService
{
    public long GetMegaBytesSizeFromFile(Stream file) => file.Length / 1000000;
    public async Task<int> GetTotalVideoDuration(Stream video)
    {
        return 2;
    }
    
    public (bool isImage, string ext) ValidateImage(Stream file)
    {
        (bool isImage, string ext) = (false, "");

        if (file.Is<JointPhotographicExpertsGroup>())
        {
            (isImage, ext) = (true, GetExtension(JointPhotographicExpertsGroup.TypeExtension));
        } else if(file.Is<PortableNetworkGraphic>())
        {
            (isImage, ext) = (true, GetExtension(PortableNetworkGraphic.TypeExtension));
        }

        file.Position = 0;

        return (isImage, ext);
    }

    public async Task<(int width, int height)> GetMediaDimensions(Stream media, MediaType type)
    {
        int height = 0, width = 0;
        
        if (type == MediaType.VIDEO)
        {
            var mediaInfo = MediaFile.Open(media);
        
            if(!mediaInfo.HasAudio)
                throw new DomainException("Media type not ");

            var size = mediaInfo.Video.Info.FrameSize;
        
            return (size.Width, size.Height);   
        }
        if (type == MediaType.IMAGE)
        {
            try
            {
                var imageInfos = await Image.LoadAsync(media);
                height = imageInfos.Height; width = imageInfos.Width;
            }
            catch (Exception e)
            {
                throw new InternalServiceException("It was not possible process media of type image");
            }
        }

        return (width, height);
    }

    private string GetExtension(string extension)
    {
        return extension.StartsWith(".") ? extension : $".{extension}";
    }

    public string GetMediaExtension(string fileName) => Path.GetExtension(fileName);
    
    private bool IsValidImage(Stream file)
    {
        try
        {
            using var image = Image.Load(file);
            file.Position = 0;
            
            return true;
        }
        catch (Exception e)
        {
            file.Position = 0;
            
            return false;
        }
    }

    private async Task<bool> IsValidVideo(Stream file)
    {
        var isValid = false;
        
        var tempFilePath = Path.GetTempFileName();
        
        using var video = File.Create(tempFilePath);
        await file.CopyToAsync(video);

        using var media = MediaFile.Open(video);
        if (media.HasVideo)
            isValid = true;
        
        file.Position = 0;
        
        return isValid;
    }

    public async Task<MediaType> GetMediaType(Stream media)
    {
        if (IsValidImage(media))
            return MediaType.IMAGE;
        if(await IsValidVideo(media))
            return MediaType.VIDEO;
        
        throw new DomainException("Media type not supported");
    }

    public async Task<bool> IsMediaValid(Stream media)
    {
        return IsValidImage(media) || await IsValidVideo(media);
    }

    public Task<string> GetMediaFullName(Stream media)
    {
        throw new NotImplementedException();
    }
}