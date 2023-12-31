using Torello.Domain.Boards;
using Torello.Domain.Common.Primitives;
using Torello.Domain.UserProjects;

namespace Torello.Domain.Projects;

public class Project : Entity<ProjectId>
{
    public string Title { get; private set; }
    public string Description { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    // Navigation
    private readonly List<UserProject> _userProjects = new();
    public virtual IReadOnlyList<UserProject> UserProjects => _userProjects.AsReadOnly();
    private readonly List<Board> _boards = new();
    public virtual IReadOnlyList<Board> Boards => _boards.AsReadOnly();

    private Project(ProjectId id, string title, string description) : base(id)
    {
        Title = title;
        Description = description;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public static Project Create(string title, string description)
    {
        var project = new Project(ProjectId.CreateUnique(), title, description);

        project.AddDomainEvent(new ProjectCreatedDomainEvent(project));

        return project;
    }

    public void Update(string title, string description)
    {
        Title = title;
        Description = description;
    }

    public void AddBoard(Board board)
    {
        _boards.Add(board);
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Project(ProjectId id) : base(id) { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
