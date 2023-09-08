using Torello.Domain.Common.Primitives;
using Torello.Domain.Projects;

namespace Torello.Domain.Users;

public sealed class User : Entity<UserId>
{
    public string Username { get; private set; }
    public string HashedPassword { get; private set; }

    // Navigation
    private readonly List<Project> _projects = new List<Project>();
    public IReadOnlyList<Project> Projects => _projects.AsReadOnly();

    private User(
        UserId id,
        string username,
        string hashedPassword
    ) : base(id)
    {
        Username = username;
        HashedPassword = hashedPassword;
    }

    public static User Create(
        string username,
        string hashedPassword
    )
    {
        return new User(
            UserId.CreateUnique(),
            username,
            hashedPassword
        );
    }
}