using Torello.Domain.Boards;

namespace Torello.Application.Common.Interfaces.Persistence;

public interface IBoardRepository : IRepository<Board, BoardId>
{
}