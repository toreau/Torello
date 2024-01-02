using Torello.Domain.Common.Primitives;
using Torello.Domain.Lanes;

namespace Torello.Domain.Issues;

public class Issue : Entity<IssueId>
{
    public string Title { get; private set; }
    public string Description { get; private set; }
    public IssuePriority Priority { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    // Navigation
    public virtual Lane Lane { get; private set; } = null!;

    private Issue(IssueId id, string title, string description, IssuePriority priority) : base(id)
    {
        Title = title;
        Description = description;
        Priority = priority;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public static Issue Create(string title, string description, IssuePriority priority)
    {
        return new Issue(IssueId.CreateUnique(), title, description, priority);
    }

    public void Update(string title, string description)
    {
        Title = title;
        Description = description;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Issue(IssueId id) : base(id)
    {
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}

public enum IssuePriority
{
    Low,
    Medium,
    High,
    Critical
}
