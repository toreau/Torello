using Torello.Domain.Projects;

namespace Torello.Application.Common.Interfaces.Persistence;

public interface IProjectRepository : IRepository<Project, ProjectId>
{
}