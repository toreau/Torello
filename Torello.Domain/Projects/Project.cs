using Torello.Domain.Boards;
using Torello.Domain.Common.Primitives;

namespace Torello.Domain.Projects;

public sealed class Project : Entity<ProjectId>
{
    public string Title { get; private set; }
    public string Description { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    // Navigation
    private readonly List<Board> _boards = new List<Board>();
    public IReadOnlyList<Board> Boards => _boards.AsReadOnly();

    private Project(
        ProjectId id,
        string title,
        string description,
        DateTimeOffset createdAt
    ) : base(id)
    {
        Title = title;
        Description = description;
        CreatedAt = createdAt;
    }

    public static Project Create(
        string title,
        string description
    )
    {
        var project = new Project(
            ProjectId.CreateUnique(),
            title,
            description,
            DateTimeOffset.UtcNow
        );

        // Add default/example board
        project.AddBoard(Board.Create("Example board"));

        project.AddDomainEvent(new ProjectCreatedDomainEvent(project));

        return project;
    }

    public void Update(
        string title,
        string description
    )
    {
        Title = title;
        Description = description;
    }

    public void AddBoard(Board board)
    {
        _boards.Add(board);
    }
}
