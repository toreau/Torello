using Torello.Domain.Boards;
using Torello.Domain.Common.Primitives;
using Torello.Domain.Issues;

namespace Torello.Domain.Lanes;

public class Lane : Entity<LaneId>
{
    public string Title { get; private set; }

    // Navigation
    public virtual Board Board { get; private set; } = null!;
    private readonly List<Issue> _issues = new();
    public virtual IReadOnlyList<Issue> Issues => _issues.AsReadOnly();

    private Lane(LaneId id, string title ) : base(id)
    {
        Title = title;
    }

    public static Lane Create(string title)
    {
        return new Lane(LaneId.CreateUnique(), title);
    }

    public void Update(string title)
    {
        Title = title;
    }

    public void AddIssue(Issue issue)
    {
        _issues.Add(issue);
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Lane(LaneId id) : base(id) { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
