using Torello.Domain.Common.Primitives;
using Torello.Domain.Projects;

namespace Torello.Domain.Users;

public class User : Entity<UserId>
{
    public string Username { get; private set; }
    public string HashedPassword { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    // Navigation
    private readonly List<Project> _projects = new();
    public virtual IReadOnlyList<Project> Projects => _projects.AsReadOnly();

    private User(UserId id, string username, string hashedPassword, DateTimeOffset createdAt) : base(id)
    {
        Username = username;
        HashedPassword = hashedPassword;
        CreatedAt = createdAt;
    }

    public static User Create(string username, string hashedPassword)
    {
        var user = new User(UserId.CreateUnique(), username, hashedPassword, DateTimeOffset.UtcNow);

        // Add default/example project
        user.AddProject(Project.Create(
            "Your first project",
            "Add sensible description of the project, if need be."
        ));

        return user;
    }

    public void AddProject(Project project)
    {
        _projects.Add(project);
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public User(UserId id) : base(id) { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
