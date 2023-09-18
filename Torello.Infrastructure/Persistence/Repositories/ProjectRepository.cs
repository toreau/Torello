using Microsoft.EntityFrameworkCore;
using Torello.Application.Common.Interfaces.Persistence;
using Torello.Domain.Projects;

namespace Torello.Infrastructure.Persistence.Repositories;

public class ProjectRepository : Repository<Project, ProjectId>, IProjectRepository
{
    public ProjectRepository(TorelloDbContext dbContext) : base(dbContext)
    {
    }

    public async override Task<IEnumerable<Project>> GetAllAsync()
    {
        return await _dbContext.Projects
            .Include(p => p.Boards)
            .ToListAsync();
    }

    public async override Task<Project?> GetByIdAsync(ProjectId id)
    {
        return await _dbContext.Projects
            .Include(p => p.Boards)
            .SingleOrDefaultAsync(p => p.Id == id);
    }
}
