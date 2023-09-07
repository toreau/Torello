using Torello.Application.Common.Interfaces.Persistence;
using Torello.Infrastructure.Persistence.Repositories;

namespace Torello.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly TorelloDbContext _dbContext;
    private bool _isDisposed;

    public IProjectRepository Projects { get; }
    public IBoardRepository Boards { get; }
    public ILaneRepository Lanes { get; }
    public IIssueRepository Issues { get; }

    public UnitOfWork(TorelloDbContext dbContext)
    {
        _dbContext = dbContext;

        Projects = new ProjectRepository(dbContext);
        Boards = new BoardRepository(dbContext);
        Lanes = new LaneRepository(dbContext);
        Issues = new IssueRepository(dbContext);
    }

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