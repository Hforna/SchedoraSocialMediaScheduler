using Microsoft.AspNetCore.Http.Features;
using Schedora.Domain.DomainServices;
using Schedora.Domain.Dtos;
using Schedora.Domain.RabbitMq.Producers;
using SocialScheduler.Domain.Constants;

namespace Schedora.Application.Services;

public interface IPostService
{
    public Task<PostResponse> CreatePost(CreatePostRequest request);
    public Task<List<PostValidationDto>> ValidatePost(long postId);
    public Task<PostValidationResponse> GetPostValidation(long postId);
    public Task PublishPost(long postId);
}

public class PostService(
    IMapper _mapper,
    ICurrentUserService _currentUser,
    IPostDomainService _postDomainService,
    IUnitOfWork _uow,
    IMediaService _mediaService,
    IActivityLogService _activityLogService,
    IPostProducer _postProducer,
    IEnumerable<IMediaValidationEngine> _mediaValidationEngines,
    ILogger<IPostService> _logger, 
    IEnumerable<IContentValidatorEngine> _contentValidatorEngine)
    : IPostService
{

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
        var approvalStatus = (request.TeamContext && user.Id != teamOwnerId) || !request.TeamContext ? ApprovalStatus.NotRequired : ApprovalStatus.Pending;
        var post = Post.Create(request.Content, teamOwnerId, PostStatus.Pending, user.Id, timezone, request.TemplateId);
        
        await _uow.GenericRepository.Add<Post>(post);
        await _uow.Commit();

        var postMedias = 
            request.Medias.Select(media => new PostMedia(post.Id, media.MediaId, media.OrderIndex, media.AltText))
                .ToList();

        var postPlatforms = socialAccounts
            .Select(account =>
            new PostPlatform(post.Id, account.Id, account.Platform))
            .ToList();
            
        await _activityLogService.LogAsync(user.Id, ActivityActions.POST_CREATED, nameof(Post), post.Id, new
        {
            OwnerId = teamOwnerId,
        }, false);
        
        await _uow.GenericRepository.AddRange<PostPlatform>(postPlatforms);
        await _uow.GenericRepository.AddRange<PostMedia>(postMedias);

        var postValidation = new PostValidation()
        {
            PostId = post.Id
        };
        await _uow.GenericRepository.Add<PostValidation>(postValidation);
        await _uow.Commit();

        await _postProducer.SendPostCreated(post.Id);

        return _mapper.Map<PostResponse>(post);
    }

    public async Task<List<PostValidationDto>> ValidatePost(long postId)
    {
        var post = await _uow.PostRepository.GetPostById(postId)
            ?? throw new NotFoundException("Post not found");

        var postValidationResponses = new List<PostValidationDto>();
        
        var postPlatforms = post.Platforms.Select(d => d.Platform).Distinct();

        var postMedias = await _uow.MediaRepository.GetPostMedias(postId);
        if (postMedias.Any())
        {
            var validateMedias = ValidatePostMedias(postMedias, postPlatforms);   
            postValidationResponses.AddRange(validateMedias);
        }

        if (!string.IsNullOrEmpty(post.Content))
        {
            var validateContent = ValidatePostContent(post, postPlatforms);
            postValidationResponses.AddRange(validateContent);   
        }

        postValidationResponses = postValidationResponses.GroupBy(d => d.Platform)
            .Select(val =>
            {
                var validationDtos = val.AsEnumerable();
                var errors = validationDtos.Select(d => d.Errors);
                var isValid = val.All(d => d.IsValid);
                
                return new PostValidationDto()
                {
                    Errors = string.Join(Environment.NewLine, errors),
                    Platform = val.Key,
                    IsValid = isValid
                };
            }).ToList();

        var postValidation = await _uow.PostRepository.GetPostValidationByPost(postId)
            ?? throw new NotFoundException("Post validation not exists");

        if (postValidationResponses.Any())
        {
            postValidation.SetValidations(postValidationResponses);
            postValidation.SucceedValidation();
        }
        
        _uow.GenericRepository.Update<PostValidation>(postValidation);
        await _uow.Commit();
        
        return postValidationResponses;
    }

    public async Task<PostValidationResponse> GetPostValidation(long postId)
    {
        var user = await _currentUser.GetUser();
        
        var post =  await _uow.PostRepository.GetPostById(postId) 
                    ?? throw new NotFoundException("Post not found");

        if(await UserCanAccessPost(post, user))
            throw new DomainException("User doesn't have permission to access this post");

        var postValidation = await _uow.PostRepository.GetPostValidationByPost(post.Id);
        
        return _mapper.Map<PostValidationResponse>(postValidation);
    }

    public async Task PublishPost(long postId)
    {
        var user =  await _currentUser.GetUser();
        
        var post = await _uow.PostRepository.GetPostById(postId) 
                   ??  throw new NotFoundException("Post not found");
        
        if(!post.CanBePublished())
            throw new DomainException("Post cannot be published");
        
        if(await UserCanAccessPost(post, user))
            throw new DomainException("User doesn't have permission to access this post");
        
        
    }

    private async Task<bool> UserCanAccessPost(Post post, User user, TeamMember? userTeam = null)
    {
        if (user.Id == post.UserId) 
            return true;
        
        userTeam = userTeam ?? await _uow.TeamMemberRepository.GetByMemberUserId(user.Id);

        if (userTeam is null || userTeam.TeamOwnerId != post.UserId)
            return false;

        return true;
    }

    private List<PostValidationDto> ValidatePostMedias(List<Media> postMedias, IEnumerable<string> postPlatforms)
    {
        var postValidationResponses = new List<PostValidationDto>();
        
        var mediaEngines = _mediaValidationEngines.Where(d => postPlatforms.Contains(d.Platform));
        if (mediaEngines.Any())
        {
            var mediasDescriptor = postMedias.Select(media => 
                new MediaDescriptorDto(media.Type, media.FileSize, media.Format, media.Duration, media.Width, media.Height));
            foreach (var mediaEngine in mediaEngines)
            {
                var (isValid, errors) = mediaEngine.IsValid(mediasDescriptor!);

                var postValidationResponse = new PostValidationDto()
                {
                    Errors = errors,
                    Platform = mediaEngine.Platform,
                    IsValid = isValid,
                };
                postValidationResponses.Add(postValidationResponse);
            }
        }

        return postValidationResponses;
    }

    private List<PostValidationDto> ValidatePostContent(Post post, IEnumerable<string>  postPlatforms)
    {
        var validatorEngines = _contentValidatorEngine.Where(d => postPlatforms.Contains(d.Platform));
    
        var validationResponses = new List<PostValidationDto>();
        
        if(!validatorEngines.Any())
            return validationResponses;
        
        foreach (var engine in validatorEngines)
        {
            var validate = engine.Validate(post.Content);

            var validationDto = new PostValidationDto()
            {
                Platform = engine.Platform,
                IsValid = validate.IsValid,
                Errors = validate.Errors
            };
            validationResponses.Add(validationDto);
        }

        return validationResponses;
    }
}