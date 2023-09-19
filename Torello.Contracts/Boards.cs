using Torello.Domain.Boards;

namespace Torello.Contracts;

public sealed record BoardResult(
    Board Board
)
{
    public BoardResponse ToResponse()
        => new BoardResponse(
            Board.Id.Value,
            Board.Title,
            Board.CreatedAt
        );
}

public sealed record BoardResponse(
    Guid Id,
    string Title,
    DateTimeOffset CreatedAt
);
