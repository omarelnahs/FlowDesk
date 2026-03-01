using FlowDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlowDesk.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User>            Users            => Set<User>();
    public DbSet<Workspace>       Workspaces       => Set<Workspace>();
    public DbSet<WorkspaceMember> WorkspaceMembers => Set<WorkspaceMember>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // Composite PK on join table
        mb.Entity<WorkspaceMember>().HasKey(m => new { m.WorkspaceId, m.UserId });

        // Unique email
        mb.Entity<User>().HasIndex(u => u.Email).IsUnique();

        // Unique workspace slug
        mb.Entity<Workspace>().HasIndex(w => w.Slug).IsUnique();
    }
}
