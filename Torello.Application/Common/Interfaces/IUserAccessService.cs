using Torello.Domain.Projects;

namespace Torello.Application.Common.Interfaces;

public interface IUserAccessService
{
    Task<bool> CurrentUserCanAccessProject(Project project);
}
