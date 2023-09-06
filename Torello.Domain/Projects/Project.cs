using Torello.Domain.Common.Primitives;

namespace Torello.Domain.Projects;

public sealed class Project : Entity<ProjectId>
{
    public string Name { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private Project(
        ProjectId id,
        string name,
        DateTimeOffset createdAt
    ) : base(id)
    {
        Name = name;
        CreatedAt = createdAt;
    }

    public static Project Create(
        string name
    )
    {
        return new Project(
            ProjectId.CreateUnique(),
            name,
            DateTimeOffset.UtcNow
        );
    }
}
