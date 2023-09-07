using Torello.Domain.Common.Primitives;
using Torello.Domain.Issues;

namespace Torello.Domain.Lanes;

public class Lane : Entity<LaneId>
{
    public string Title { get; private set; }

    // Navigation
    private readonly List<Issue> _issues = new List<Issue>();
    public IReadOnlyList<Issue> Issues => _issues.AsReadOnly();

    private Lane(
        LaneId id,
        string title
    ) : base(id)
    {
        Title = title;
    }

    public static Lane Create(
        string title
    )
    {
        return new Lane(
            LaneId.CreateUnique(),
            title
        );
    }
}