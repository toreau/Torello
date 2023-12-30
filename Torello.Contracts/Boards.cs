using System.Data.SqlTypes;
using Torello.Domain.Boards;

namespace Torello.Contracts;

// Single
public sealed record BoardResult(Board Board)
{
    public BoardResponse ToResponse() => new(Board.Id.Value, Board.Title, Board.CreatedAt);
}

public sealed record BoardResponse(SqlGuid Id, string Title, DateTimeOffset CreatedAt);

// Multiple
public sealed record BoardsResult(IEnumerable<Board> Boards)
{
    public BoardsResponse ToResponse() => new(Boards.Select(board => new BoardResult(board).ToResponse()));
}

public sealed record BoardsResponse(IEnumerable<BoardResponse> Boards);
