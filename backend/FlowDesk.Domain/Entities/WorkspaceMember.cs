using System;

namespace FlowDesk.Domain.Entities;

public enum MemberRole { Owner, Admin, Member }

public class WorkspaceMember
{
    public Guid WorkspaceId { get; set; }
    public Guid UserId { get; set; }
    public MemberRole Role { get; set; } = MemberRole.Member;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Workspace Workspace { get; set; } = default!;
    public User User { get; set; } = default!;
}
