using Microsoft.EntityFrameworkCore;
using Torello.Application.Common.Interfaces.Persistence;
using Torello.Domain.Projects;

namespace Torello.Infrastructure.Persistence.Repositories;

public class ProjectRepository : Repository<Project, ProjectId>, IProjectRepository
{
    public ProjectRepository(TorelloDbContext dbContext) : base(dbContext)
    {
    }
}
