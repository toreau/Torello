using Microsoft.EntityFrameworkCore;
using Torello.Domain.Projects;

namespace Torello.Infrastructure.Persistence.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly TorelloDbContext _dbContext;

    public ProjectRepository(TorelloDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Project>> GetAll()
    {
        return await _dbContext.Projects.ToListAsync();
    }

    public async Task<Project?> GetByIdAsync(ProjectId projectId)
    {
        return await _dbContext.Projects
            .SingleOrDefaultAsync(p => p.Id == projectId);
    }

    public async Task AddAsync(Project project)
    {
        await _dbContext.Projects.AddAsync(project);
    }
}