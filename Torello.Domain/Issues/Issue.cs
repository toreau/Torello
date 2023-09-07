using Torello.Domain.Common.Primitives;

namespace Torello.Domain.Issues;

public sealed class Issue : Entity<IssueId>
{
    public string Title { get; private set; }
    public string Description { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    private Issue(
        IssueId id,
        string title,
        string description,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt
    ) : base(id)
    {
        Title = title;
        Description = description;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public static Issue Create(
        string title,
        string description
    )
    {
        return new Issue(
            IssueId.CreateUnique(),
            title,
            description,
            DateTimeOffset.UtcNow,
            null
        );
    }
}