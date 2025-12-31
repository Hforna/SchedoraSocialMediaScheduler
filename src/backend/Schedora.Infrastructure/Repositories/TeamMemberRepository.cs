using Microsoft.EntityFrameworkCore;
using Schedora.Domain.Entities;

namespace Schedora.Infrastructure.Repositories;

public class TeamMemberRepository : BaseRepository, ITeamMemberRepository
{
    public TeamMemberRepository(DataContext context) : base(context)
    {
    }


    public async Task<TeamMember?> GetTeamMemberByUsers(long teamOwner, long memberId)
    {
        return await _context.TeamMembers.SingleOrDefaultAsync(d => d.TeamOwnerId == teamOwner && d.MemberUserId == memberId);
    }

    public async Task<TeamMember?> GetByMemberUserId(long userId)
    {
        return await _context.TeamMembers.SingleOrDefaultAsync(d => d.MemberUserId == userId);
    }
}