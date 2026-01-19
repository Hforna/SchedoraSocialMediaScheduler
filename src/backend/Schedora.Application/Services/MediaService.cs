using FFmpeg.AutoGen;
using Schedora.Domain.DomainServices;
using Schedora.Domain.Dtos;

namespace Schedora.Application.Services;

public interface IMediaService
{
    public Task<MediaResponse> UploadMedia(UploadMediaRequest request);
}

public class MediaService : IMediaService
{
    public MediaService(IStorageService storageService, IMapper mapper, 
        IUnitOfWork uow, ICurrentUserService currentUser, 
        IMediaDomainService mediaDomainService, IMediaHandlerService mediaHandlerService)
    {
        _storageService = storageService;
        _mapper = mapper;
        _uow = uow;
        _currentUser = currentUser;
        _mediaDomainService = mediaDomainService;
        _mediaHandlerService = mediaHandlerService;
    }

    private readonly IStorageService  _storageService;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly IMediaDomainService _mediaDomainService;
    private readonly IMediaHandlerService _mediaHandlerService;
    
    public async Task<MediaResponse> UploadMedia(UploadMediaRequest request)
    {
        var user = await _currentUser.GetUser();
        
        var mediaStream = request.Media.OpenReadStream();
        
        var isMediaValid = await _mediaHandlerService.IsValid(mediaStream);
        if(!isMediaValid)
            throw new RequestException("Invalid media type");
        
        var mediaExtension = _mediaHandlerService.GetExtension(request.Media.FileName);
        var mediaType = await _mediaHandlerService.GetType(mediaStream);
        var mimeType = request.Media.ContentType;
        var mediaInfos = await GetMediaInfos(mediaStream, mediaType);
        
        await _mediaDomainService.ValidateUserUploadingMedia(user, user.Subscription, mediaInfos.Size, mediaType, mediaInfos.Duration);
        
        var mediaOriginalName = $"{request.Media.FileName}";
        var mediaName = DefineMediaName(request.MediaName, mediaExtension);
        
        var mediaBuilder = new MediaBuilder(user.Id, mediaName, mediaInfos.Size, mimeType, "");
        mediaBuilder.WithDescription(request.Description);
        mediaBuilder.WithDuration(mediaInfos.Duration);
        mediaBuilder.WithDimensions(mediaInfos.Width,  mediaInfos.Height);
        mediaBuilder.WithOriginalFileName(mediaOriginalName);
        if (request.FolderId is not null)
        {
            var folder = await _uow.MediaRepository.GetMediaFolderByIdAndUser(request.FolderId.Value, user.Id)
                ?? throw new RequestException("Folder was not found");
            
            mediaBuilder.WithFolder(folder.Id);
        }
        if (mediaType == MediaType.VIDEO)
        {
            if (request.Thumbnail is not null)
            {
                var thumbnail = request.Thumbnail.OpenReadStream();
                var isImage= await _mediaHandlerService.IsValid(thumbnail);
                
                if (!isImage)
                    throw new RequestException("Invalid image type");
                
                var thumbnailName = $"thumbnail_{user.Id}_{mediaName}";
                mediaBuilder.WithThumbnailName(thumbnailName);
                
                await _storageService.UploadMedia(thumbnail, thumbnailName, user.Id);
            }
        }

        if (mediaType == MediaType.IMAGE)
        {
            await _storageService.UploadMedia(mediaStream, mediaName, user.Id);
            mediaBuilder.WithProcessing(true, ProcessingStatus.Completed, string.Empty);
        }

        var media = mediaBuilder.Build();
        
        await _uow.GenericRepository.Add<Media>(media);
        await _uow.Commit();
        
        return _mapper.Map<MediaResponse>(media);
    }

    private string DefineMediaName(string mediaName, string ext)
    {
        return $"{mediaName}_{Guid.NewGuid()}{ext}";
    }
    

    private async Task<MediaInfosDto> GetMediaInfos(Stream media, MediaType type)
    {
        int? videoDuration = null;
        if(type == MediaType.VIDEO)
            videoDuration = await _mediaHandlerService.GetTotalVideoDuration(media);
        var mediaSize = _mediaHandlerService.GetMegaBytesSizeFromFile(media);
        var mediaDimensions = await _mediaHandlerService.GetDimensions(media, type);

        return new MediaInfosDto()
        {
            Duration = videoDuration,
            Size = mediaSize,
            Width = mediaDimensions.width,
            Height = mediaDimensions.height,
        };
    }
}