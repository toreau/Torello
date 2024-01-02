using Torello.Domain.Projects;
using Torello.Domain.Users;

namespace Torello.Domain.UserProjects;

public class UserProject
{
    public UserId UserId { get; private set; } = null!;
    public ProjectId ProjectId { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
    public UserProjectRole Role { get; private set; }

    // Navigation
    public virtual User User { get; private set; }
    public virtual Project Project { get; private set; }

    private UserProject(User user, Project project, UserProjectRole role)
    {
        User = user;
        Project = project;
        CreatedAt = DateTimeOffset.UtcNow;
        Role = role;
    }

    public static UserProject Create(User user, Project project, UserProjectRole role)
    {
        return new UserProject(user, project, role);
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public UserProject() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

}

public enum UserProjectRole
{
    Owner,
    Collaborator,
    Viewer,
}
