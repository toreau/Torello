using Torello.Domain.Common.Primitives;
using Torello.Domain.Lanes;

namespace Torello.Domain.Boards;

public class Board : Entity<BoardId>
{
    public string Name { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    // Navigation
    private readonly List<Lane> _lanes = new List<Lane>();
    public IReadOnlyList<Lane> Lanes => _lanes.AsReadOnly();

    private Board(
        BoardId id,
        string name,
        DateTimeOffset createdAt
    ) : base(id)
    {
        Name = name;
        CreatedAt = createdAt;
    }

    public static Board Create(
        string name
    )
    {
        return new Board(
            BoardId.CreateUnique(),
            name,
            DateTimeOffset.UtcNow
        );
    }
}
