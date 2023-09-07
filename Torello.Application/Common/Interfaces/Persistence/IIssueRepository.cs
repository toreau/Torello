using Torello.Domain.Issues;

namespace Torello.Application.Common.Interfaces.Persistence;

public interface IIssueRepository : IRepository<Issue, IssueId>
{
}