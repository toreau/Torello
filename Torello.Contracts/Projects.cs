using System.Data.SqlTypes;
using Torello.Domain.Projects;

namespace Torello.Contracts;

// Single
public sealed record ProjectResult(Project Project)
{
    public ProjectResponse ToResponse() => new(Project.Id.Value.ToGuid(), Project.Title, Project.Description, Project.CreatedAt);
}

public sealed record ProjectResponse(Guid Id, string Title, string Description, DateTimeOffset CreatedAt);

// Multiple
public sealed record ProjectsResult(IEnumerable<Project> Projects)
{
    public ProjectsResponse ToResponse() => new(Projects.Select(project => new ProjectResult(project).ToResponse()));
}

public sealed record ProjectsResponse(IEnumerable<ProjectResponse> Projects);
