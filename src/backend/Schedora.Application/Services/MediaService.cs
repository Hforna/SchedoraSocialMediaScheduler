using FFmpeg.AutoGen;
using Schedora.Domain.DomainServices;
using Schedora.Domain.Dtos;

namespace Schedora.Application.Services;

public interface IMediaService
{
    public Task<MediaResponse> UploadMedia(UploadMediaRequest request);
}

public class MediaService(
    IStorageService storageService,
    IMapper mapper,
    IUnitOfWork uow,
    ICurrentUserService currentUser,
    IMediaDomainService mediaDomainService,
    IMediaHandlerService mediaHandlerService)
    : IMediaService
{
    public async Task<MediaResponse> UploadMedia(UploadMediaRequest request)
    {
        var user = await currentUser.GetUser();
        
        var mediaStream = request.Media.OpenReadStream();
        
        var isMediaValid = await mediaHandlerService.IsValid(mediaStream);
        if(!isMediaValid)
            throw new RequestException("Invalid media type");
        
        var mediaExtension = mediaHandlerService.GetExtension(request.Media.FileName);
        var mediaType = await mediaHandlerService.GetType(mediaStream);
        var mimeType = request.Media.ContentType;
        var mediaInfos = await GetMediaInfos(mediaStream, mediaType);
        
        await mediaDomainService.ValidateUserUploadingMedia(user, user.Subscription, mediaInfos.Size, mediaType, mediaInfos.Duration);
        
        var mediaOriginalName = $"{request.Media.FileName}";
        var mediaName = DefineMediaName(request.MediaName, mediaExtension);
        
        var mediaBuilder = new MediaBuilder(user.Id, mediaName, mediaInfos.Size, mimeType, "", mediaType, mediaInfos.Width,  mediaInfos.Height);
        mediaBuilder.WithDescription(request.Description);
        mediaBuilder.WithDuration(mediaInfos.Duration);
        mediaBuilder.WithOriginalFileName(mediaOriginalName);
        if (request.FolderId is not null)
        {
            var folder = await uow.MediaRepository.GetMediaFolderByIdAndUser(request.FolderId.Value, user.Id)
                ?? throw new RequestException("Folder was not found");
            
            mediaBuilder.WithFolder(folder.Id);
        }
        if (mediaType == MediaType.VIDEO)
        {
            if (request.Thumbnail is not null)
            {
                var thumbnail = request.Thumbnail.OpenReadStream();
                var isImage= await mediaHandlerService.IsValid(thumbnail);
                
                if (!isImage)
                    throw new RequestException("Invalid image type");
                
                var thumbnailName = $"thumbnail_{user.Id}_{mediaName}";
                mediaBuilder.WithThumbnailName(thumbnailName);
                
                await storageService.UploadMedia(thumbnail, thumbnailName, user.Id);
            }
        }

        if (mediaType == MediaType.IMAGE)
        {
            await storageService.UploadMedia(mediaStream, mediaName, user.Id);
            mediaBuilder.WithProcessing(true, ProcessingStatus.Completed, string.Empty);
        }

        var media = mediaBuilder.Build();
        
        await uow.GenericRepository.Add<Media>(media);
        await uow.Commit();
        
        return mapper.Map<MediaResponse>(media);
    }

    private string DefineMediaName(string mediaName, string ext)
    {
        return $"{mediaName}_{Guid.NewGuid()}{ext}";
    }
    

    private async Task<MediaInfosDto> GetMediaInfos(Stream media, MediaType type)
    {
        int? videoDuration = null;
        if(type == MediaType.VIDEO)
            videoDuration = await mediaHandlerService.GetTotalVideoDuration(media);
        var mediaSize = mediaHandlerService.GetMegaBytesSizeFromFile(media);
        var mediaDimensions = await mediaHandlerService.GetDimensions(media, type);

        return new MediaInfosDto()
        {
            Duration = videoDuration,
            Size = mediaSize,
            Width = mediaDimensions.width,
            Height = mediaDimensions.height,
        };
    }
}