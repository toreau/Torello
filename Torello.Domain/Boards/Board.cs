using Torello.Domain.Common.Primitives;
using Torello.Domain.Lanes;

namespace Torello.Domain.Boards;

public sealed class Board : Entity<BoardId>
{
    public string Title { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    // Navigation
    private readonly List<Lane> _lanes = new List<Lane>();
    public IReadOnlyList<Lane> Lanes => _lanes.AsReadOnly();

    private Board(
        BoardId id,
        string title,
        DateTimeOffset createdAt
    ) : base(id)
    {
        Title = title;
        CreatedAt = createdAt;
    }

    public static Board Create(
        string title
    )
    {
        return new Board(
            BoardId.CreateUnique(),
            title,
            DateTimeOffset.UtcNow
        );
    }

    public void AddLane(Lane lane)
    {
        _lanes.Add(lane);
    }
}
