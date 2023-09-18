using Torello.Domain.Projects;
using Torello.Domain.Users;

namespace Torello.Application.Common.Interfaces.Persistence;

public interface IProjectRepository : IRepository<Project, ProjectId>
{
}
