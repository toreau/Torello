using Torello.Domain.Boards;
using Torello.Domain.Common.Primitives;
using Torello.Domain.Users;

namespace Torello.Domain.Projects;

public class Project : Entity<ProjectId>
{
    public string Title { get; private set; }
    public string Description { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    // Navigation
    public virtual User User { get; private set; } = null!;
    private readonly List<Board> _boards = new List<Board>();
    public virtual IReadOnlyList<Board> Boards => _boards.AsReadOnly();

    private Project(ProjectId id, string title, string description,DateTimeOffset createdAt) : base(id)
    {
        Title = title;
        Description = description;
        CreatedAt = createdAt;
    }

    public static Project Create(string title, string description)
    {
        var project = new Project(ProjectId.CreateUnique(), title, description, DateTimeOffset.UtcNow);

        // Add default/example board
        project.AddBoard(Board.Create("Example board"));

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
