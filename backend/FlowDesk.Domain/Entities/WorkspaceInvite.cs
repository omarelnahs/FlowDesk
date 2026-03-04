using System;

namespace FlowDesk.Domain.Entities;

public class WorkspaceInvite
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkspaceId { get; set; }
    public string Email { get; set; } = default!;
    public MemberRole Role { get; set; } = MemberRole.Member;
    public string Token { get; set; } = Guid.NewGuid().ToString("N");
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(7);
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Workspace Workspace { get; set; } = default!;
}
