using Torello.Domain.Projects;

namespace Torello.Application.Common.Interfaces.Persistence;

public interface IUnitOfWork : IDisposable
{
    IProjectRepository Projects { get; }

    Task<int> SaveChangesAsync();
}