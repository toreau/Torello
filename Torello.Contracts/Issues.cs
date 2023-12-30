using System.Data.SqlTypes;
using Torello.Domain.Issues;

namespace Torello.Contracts;

// Single
public sealed record IssueResult(Issue Issue)
{
    public IssueResponse ToResponse() => new(Issue.Id.Value, Issue.Title, Issue.Description, Issue.CreatedAt, Issue.UpdatedAt);
}

public sealed record IssueResponse(SqlGuid Id, string Title, string Description, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);

// Multiple
public sealed record IssuesResult(IEnumerable<Issue> Issues)
{
    public IssuesResponse ToResponse() => new(Issues.Select(issue => new IssueResult(issue).ToResponse()));
}

public sealed record IssuesResponse(IEnumerable<IssueResponse> Issues);
