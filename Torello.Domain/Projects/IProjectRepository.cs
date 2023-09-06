namespace Torello.Domain.Projects;

public interface IProjectRepository
{
    Task<IEnumerable<Project>> GetAll();
    Task<Project?> GetByIdAsync(ProjectId projectId);
    Task AddAsync(Project project);
}