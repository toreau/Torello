using Torello.Application.Common.Interfaces;
using Torello.Domain.Projects;
using Torello.Domain.Users;

namespace Torello.Infrastructure.Authentication;

public sealed class UserAccessService(IAuthService authService) : IUserAccessService
{
    public async Task<bool> CurrentUserCanAccess(Project project)
    {
        return UserCanAccess(await authService.GetCurrentUserAsync(), project);
    }

    private bool UserCanAccess(User? user, Project project)
    {
        return user != null && user.Equals(project.User);
    }
}
