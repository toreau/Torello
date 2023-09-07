using Torello.Application.Common.Interfaces.Persistence;
using Torello.Domain.Boards;

namespace Torello.Infrastructure.Persistence.Repositories;

public class BoardRepository : Repository<Board, BoardId>, IBoardRepository
{
    public BoardRepository(TorelloDbContext dbContext) : base(dbContext)
    {
    }
}