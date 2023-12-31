using Torello.Application.Common.Interfaces;
using Torello.Domain.Projects;
using Torello.Domain.UserProjects;
using Torello.Domain.Users;

namespace Torello.Infrastructure.Authentication;

public sealed class UserAccessService(IAuthService authService) : IUserAccessService
{
    public async Task<bool> CurrentUserCanAccessProject(Project project)
    {
        return UserCanViewProject(await authService.GetCurrentUserAsync(), project);
    }

    public async Task<bool> CurrentUserCanEditProject(Project project)
    {
        return UserCanEditProject(await authService.GetCurrentUserAsync(), project);
    }

    private static bool UserCanViewProject(User? user, Project project)
    {
        return user is not null && user.UserProjects.Any(up => up.ProjectId == project.Id);
    }

    private static bool UserCanEditProject(User? user, Project project)
    {
        return user is not null && user.UserProjects.Any(up => up.ProjectId == project.Id
                                                               && (up.Role is UserProjectRole.Owner
                                                                           or UserProjectRole.Collaborator));
    }
}
