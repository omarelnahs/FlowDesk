using FlowDesk.Domain.Entities;
using FlowDesk.Domain.ValueObjects;
using FlowDesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FlowDesk.Application.Workspaces;

public class WorkspacesService(AppDbContext db)
{
    // --- Create ------------------------------------------------------------------
    public async Task<WorkspaceDto> CreateAsync(string name, Guid ownerId)
    {
        // Guarantee slug uniqueness
        var baseSlug = Slug.Generate(name);
        var slug     = baseSlug;

        var counter = 1;
        while (await db.Workspaces.AnyAsync(w => w.Slug == slug))
            slug = $"{baseSlug}-{counter++}";

        var workspace = new Workspace { Name = name, Slug = slug };
        db.Workspaces.Add(workspace);

        db.WorkspaceMembers.Add(new WorkspaceMember
        {
            WorkspaceId = workspace.Id,
            UserId      = ownerId,
            Role        = MemberRole.Owner,
        });

        await db.SaveChangesAsync();
        return ToDto(workspace, MemberRole.Owner);
    }

    // --- Read --------------------------------------------------------------------
    public async Task<List<WorkspaceDto>> GetForUserAsync(Guid userId)
        => await db.WorkspaceMembers
            .Where(m => m.UserId == userId)
            .Select(m => new WorkspaceDto(
                m.Workspace.Id,
                m.Workspace.Name,
                m.Workspace.Slug,
                m.Role.ToString()))
            .ToListAsync();

    public async Task<WorkspaceMember?> GetMembershipAsync(Guid userId, string slug)
        => await db.WorkspaceMembers
            .Include(m => m.Workspace)
            .FirstOrDefaultAsync(m =>
                m.Workspace.Slug == slug &&
                m.UserId == userId);

    // --- Helper ------------------------------------------------------------------
    private static WorkspaceDto ToDto(Workspace w, MemberRole role)
        => new(w.Id, w.Name, w.Slug, role.ToString());
}

public record WorkspaceDto(Guid Id, string Name, string Slug, string Role);
