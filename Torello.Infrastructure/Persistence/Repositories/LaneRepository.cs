using Torello.Application.Common.Interfaces.Persistence;
using Torello.Domain.Lanes;

namespace Torello.Infrastructure.Persistence.Repositories;

public class LaneRepository : Repository<Lane, LaneId>, ILaneRepository
{
    public LaneRepository(TorelloDbContext dbContext) : base(dbContext)
    {
    }
}