namespace Torello.Domain.Projects;

public sealed class Project
{
    public ProjectId Id { get; private set; }
    public string Name { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private Project(
        ProjectId id,
        string name,
        DateTimeOffset createdAt
    )
    {
        Id = id;
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
