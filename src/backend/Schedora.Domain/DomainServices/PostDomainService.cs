using Schedora.Domain.Exceptions;
using Schedora.Domain.Interfaces;

namespace Schedora.Domain.DomainServices;

public interface IPostDomainService
{
    public void IsPostScheduleValid(DateTime schedule);
    public Task<bool> UserHasPermissionToPost(SocialAccount  socialAccount, User user);
}

public class PostDomainService : IPostDomainService
{
    private readonly IUnitOfWork _uow;

    public PostDomainService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public void IsPostScheduleValid(DateTime schedule)
    {
        if (schedule <= DateTime.UtcNow)
            throw new DomainException("Post can't be scheduled on the past");
        
        if(schedule >= DateTime.UtcNow.AddDays(70))
            throw new DomainException("Post cannot be scheduled after 70 days");
    }

    public async Task<bool> UserHasPermissionToPost(SocialAccount socialAccount, User user)
    {
        if (socialAccount.UserId == user.Id)
            return true;
        
        var teamMember = await _uow.TeamMemberRepository.GetTeamMemberByUsers(socialAccount.UserId, user.Id);

        if (teamMember is null || teamMember.Role == TeamRole.Viewer)
            return false;
        
        return true;
    }
}