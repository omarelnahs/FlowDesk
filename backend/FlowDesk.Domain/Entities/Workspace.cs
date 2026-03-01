using System;
using System.Collections.Generic;

namespace FlowDesk.Domain.Entities;

public class Workspace
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!; // e.g. "my-team"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<WorkspaceMember> Members { get; set; } = [];
}
