using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schedora.Domain.Entities
{
    public class TeamMember : Entity
    {
        // Properties
        public long TeamOwnerId { get; private set; }
        public long MemberUserId { get; private set; }
        public TeamRole Role { get; private set; }
        public DateTime InvitedAt { get; private set; }
        public DateTime? JoinedAt { get; private set; }
        public string InviteStatus { get; private set; } // pending, accepted, declined

        // Navigation Properties
        public virtual User TeamOwner { get; private set; }
        public virtual User MemberUser { get; private set; }

        // Private constructor for EF
        private TeamMember() { }

        // Factory method
        public static TeamMember Create(long teamOwnerId, long memberUserId, TeamRole role)
        {
            return new TeamMember
            {
                TeamOwnerId = teamOwnerId,
                MemberUserId = memberUserId,
                Role = role,
                InvitedAt = DateTime.UtcNow,
                InviteStatus = "pending"
            };
        }

        // Domain methods
        public void AcceptInvite()
        {
            if (InviteStatus != "pending")
                throw new InvalidOperationException("Invite has already been responded to");

            InviteStatus = "accepted";
            JoinedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void DeclineInvite()
        {
            if (InviteStatus != "pending")
                throw new InvalidOperationException("Invite has already been responded to");

            InviteStatus = "declined";
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateRole(TeamRole newRole)
        {
            Role = newRole;
            UpdatedAt = DateTime.UtcNow;
        }

        public bool CanApprove() => Role == TeamRole.Manager || Role == TeamRole.Admin;
        public bool CanManageTeam() => Role == TeamRole.Admin;
        public bool CanCreatePosts() => Role != TeamRole.Viewer;
    }
}
