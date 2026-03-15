using FlowDesk.Domain.Entities;
using FlowDesk.Infrastructure.Persistence;
using FlowDesk.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FlowDesk.Application.Invites;

public class InviteService(AppDbContext db, EmailService email, IConfiguration config)
{
    // Owner/Admin sends an invite
    public async Task<InviteResult> SendAsync(
        string workspaceSlug, Guid senderId, string toEmail, MemberRole role)
    {
        var workspace = await db.Workspaces
            .FirstOrDefaultAsync(w => w.Slug == workspaceSlug)
            ?? throw new KeyNotFoundException("Workspace not found");

        // Verify sender is at least Admin
        var sender = await db.WorkspaceMembers
            .FirstOrDefaultAsync(m => 
                m.WorkspaceId == workspace.Id && m.UserId == senderId)
            ?? throw new UnauthorizedAccessException();

        if (sender.Role == MemberRole.Member)
            throw new UnauthorizedAccessException("Members cannot invite");

        var invite = new WorkspaceInvite
        {
            WorkspaceId = workspace.Id,
            Email       = toEmail.ToLower(),
            Role        = role,
        };
        db.WorkspaceInvites.Add(invite);
        await db.SaveChangesAsync();

        var url = $"{config["App:FrontendUrl"]}/invite/{invite.Token}";
        await email.SendInviteAsync(toEmail, workspace.Name, url);

        return new InviteResult(invite.Token, invite.ExpiresAt);
    }

    // Invited user clicks the link and accepts
    public async Task<string> AcceptAsync(string token, Guid userId)
    {
        var invite = await db.WorkspaceInvites
            .Include(i => i.Workspace)
            .FirstOrDefaultAsync(i => i.Token == token)
            ?? throw new KeyNotFoundException("Invalid invite token");

        if (invite.ExpiresAt < DateTime.UtcNow)
            throw new InvalidOperationException("Invite has expired");

        var alreadyMember = await db.WorkspaceMembers
            .AnyAsync(m => 
                m.WorkspaceId == invite.WorkspaceId && m.UserId == userId);

        if (!alreadyMember)
        {
            db.WorkspaceMembers.Add(new WorkspaceMember
            {
                WorkspaceId = invite.WorkspaceId,
                UserId      = userId,
                Role        = invite.Role,
            });
        }

        db.WorkspaceInvites.Remove(invite); // one-time use
        await db.SaveChangesAsync();

        return invite.Workspace.Slug;
    }
}

public record InviteResult(string Token, DateTime ExpiresAt);
