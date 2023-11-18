using Torello.Domain.Common.Primitives;
using Torello.Domain.Lanes;
using Torello.Domain.Projects;
using Torello.Domain.Users;

namespace Torello.Domain.Boards;

public class Board : Entity<BoardId>
{
    public string Title { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    // Navigation
    public virtual Project Project { get; private set; } = null!;
    public virtual User CreatedBy { get; private set; } = null!;
    private readonly List<Lane> _lanes = new();
    public virtual IReadOnlyList<Lane> Lanes => _lanes.AsReadOnly();

    private Board(BoardId id, string title, DateTimeOffset createdAt, User createdBy) : base(id)
    {
        Title = title;
        CreatedAt = createdAt;
        CreatedBy = createdBy;
    }

    public static Board Create(string title, User createdBy)
    {
        var board = new Board(BoardId.CreateUnique(), title, DateTimeOffset.UtcNow, createdBy);

        // Add some default/example lanes
        foreach (var laneTitle in new[] { "Backlog", "Todo", "Doing", "Done" })
            board.AddLane(Lane.Create(laneTitle));

        return board;
    }

    public void Update(string title)
    {
        Title = title;
    }

    public User Owner => Project.Owner;

    public void AddLane(Lane lane)
    {
        _lanes.Add(lane);
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Board(BoardId id) : base(id)
    {
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
