using Torello.Domain.Common.Primitives;

namespace Torello.Domain.Issues;

public sealed class Issue : Entity<IssueId>
{
    public string Title { get; private set; }
    public string Description { get; private set; }
    public IssuePriority Priority { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    private Issue(
        IssueId id,
        string title,
        string description,
        IssuePriority priority,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt
    ) : base(id)
    {
        Title = title;
        Description = description;
        Priority = priority;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public static Issue Create(
        string title,
        string description,
        IssuePriority priority
    )
    {
        return new Issue(
            IssueId.CreateUnique(),
            title,
            description,
            priority,
            DateTimeOffset.UtcNow,
            null
        );
    }
}

public enum IssuePriority
{
    Low,
    Medium,
    High,
    Critical
}