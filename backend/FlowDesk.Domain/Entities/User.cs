using System;
using System.Collections.Generic;

namespace FlowDesk.Domain.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? PasswordHash { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<WorkspaceMember> Memberships { get; set; } = [];
}
