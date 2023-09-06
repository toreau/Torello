using Torello.Application.Common.Interfaces.Persistence;
using Torello.Infrastructure.Persistence.Repositories;

namespace Torello.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly TorelloDbContext _dbContext;
    private bool _isDisposed;

    public UnitOfWork(TorelloDbContext dbContext)
    {
        _dbContext = dbContext;

        Projects = new ProjectRepository(dbContext);
    }

    public IProjectRepository Projects { get; }

    public async Task<int> SaveChangesAsync()
    {
        return await _dbContext.SaveChangesAsync();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_isDisposed)
            return;

        if (disposing)
            _dbContext.Dispose();

        _isDisposed = true;
    }
}