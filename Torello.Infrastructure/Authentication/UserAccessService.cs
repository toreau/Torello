using Torello.Application.Common.Interfaces;
using Torello.Domain.Projects;
using Torello.Domain.Users;

namespace Torello.Infrastructure.Authentication;

public sealed class UserAccessService(IAuthService authService) : IUserAccessService
{
    public async Task<bool> CurrentUserCanAccessProject(Project project)
    {
        return UserCanAccessProject(await authService.GetCurrentUserAsync(), project);
    }

    private bool UserCanAccessProject(User? user, Project project)
    {
        return user != null && user.Equals(project.User);
    }
}
