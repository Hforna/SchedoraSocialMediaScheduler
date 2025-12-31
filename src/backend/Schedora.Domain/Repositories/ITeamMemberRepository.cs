namespace Schedora.Domain.Interfaces;

public interface ITeamMemberRepository
{
    public Task<TeamMember?> GetTeamMemberByUsers(long teamOwner, long memberId);
}