using Torello.Application.Common.Interfaces.Persistence;
using Torello.Domain.Issues;

namespace Torello.Infrastructure.Persistence.Repositories;

public class IssueRepository : Repository<Issue, IssueId>, IIssueRepository
{
    public IssueRepository(TorelloDbContext dbContext) : base(dbContext)
    {
    }
}