using Microsoft.AspNetCore.Http.Features;
using Schedora.Domain.DomainServices;
using SocialScheduler.Domain.Constants;

namespace Schedora.Application.Services;

public interface IPostService
{
    public Task<PostResponse> CreatePost(CreatePostRequest request);
}

public class PostService : IPostService
{
    public PostService(IMapper mapper, ICurrentUserService currentUser, 
        IPostDomainService postDomainService, IUnitOfWork uow, 
        IMediaService mediaService, IActivityLogService activityLogService)
    {
        _mapper = mapper;
        _currentUser = currentUser;
        _postDomainService = postDomainService;
        _uow = uow;
        _mediaService = mediaService;
        _activityLogService = activityLogService;
    }

    private readonly IMapper _mapper;
    private readonly ICurrentUserService  _currentUser;
    private readonly IPostDomainService _postDomainService;
    private readonly IUnitOfWork _uow;
    private readonly IMediaService _mediaService;
    private readonly IActivityLogService  _activityLogService;
    
    public async Task<PostResponse> CreatePost(CreatePostRequest request)
    {
        var user = await _currentUser.GetUser();

        long teamOwnerId = user.Id;
        if (request.TeamContext)
        {
            var teamMember = await _uow.TeamMemberRepository.GetByMemberUserId(user.Id);
            
            if(teamMember is null)
                throw new RequestException("User is not part of a team");

            if (!teamMember.CanCreatePosts())
                throw new DomainException("User are not able to create posts");
            
            teamOwnerId = teamMember.TeamOwnerId;
        }
        
        var medias = await _uow.MediaRepository.GetMediasByIds(request.Medias!.Select(d => d.MediaId).ToList(), user.Id);
        
        if(medias.Count != request.Medias.Count)
            throw new RequestException("User haven't uploaded some of these medias yet");

        var socialAccounts = await _uow.SocialAccountRepository.GetSocialAccountsByIds(request.SocialAccountsIds);

        if (!socialAccounts.Any() || socialAccounts.Count != request.SocialAccountsIds.Count)
            throw new RequestException("Invalid social accounts");

        if (request.TeamContext && socialAccounts.Any(d => d.UserId != teamOwnerId))
        {
            throw new DomainException("Social accounts must be of the team owner");   
        }
        else
        {
            if(socialAccounts.Any(d => d.UserId != user.Id))
                throw new RequestException("User is not part of a team");   
        }

        var timezone = _currentUser.GetCurrentUserTimeZone();
        var post = Post.Create(request.Content, teamOwnerId, PostStatus.Pending, user.Id, timezone, request.TemplateId);
        
        await _uow.GenericRepository.Add<Post>(post);
        await _uow.Commit();

        var postMedias = 
            request.Medias.Select(media => new PostMedia(post.Id, media.MediaId, media.OrderIndex, media.AltText))
                .ToList();

        var postPlatforms = socialAccounts
            .Select(account =>
            new PostPlatform(post.Id, account.Id, Enum.Parse<Platform>(account.Platform, true)))
            .ToList();
            
        await _activityLogService.LogAsync(user.Id, ActivityActions.POST_CREATED, nameof(Post), post.Id, new
        {
            OwnerId = teamOwnerId,
        }, false);
        
        await _uow.GenericRepository.AddRange<PostPlatform>(postPlatforms);
        await _uow.GenericRepository.AddRange<PostMedia>(postMedias);
        await _uow.Commit();

        return _mapper.Map<PostResponse>(post);
    }
}