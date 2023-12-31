using Torello.Domain.Projects;

namespace Torello.Application.Common.Interfaces;

public interface IUserAccessService
{
    Task<bool> CurrentUserCanViewProject(Project project);
    Task<bool> CurrentUserCanEditProject(Project project);
}
