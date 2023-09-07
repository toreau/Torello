using MediatR;
using Torello.Application.Common.Interfaces.Persistence;
using Torello.Domain.Common.Primitives;
using Torello.Domain.Common.ValueObjects;
using Torello.Infrastructure.Persistence.Repositories;

namespace Torello.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly TorelloDbContext _dbContext;
    private readonly IPublisher _publisher;

    private bool _isDisposed;

    public IProjectRepository Projects { get; }
    public IBoardRepository Boards { get; }
    public ILaneRepository Lanes { get; }
    public IIssueRepository Issues { get; }

    public UnitOfWork(TorelloDbContext dbContext, IPublisher publisher)
    {
        _dbContext = dbContext;
        _publisher = publisher;

        Projects = new ProjectRepository(dbContext);
        Boards = new BoardRepository(dbContext);
        Lanes = new LaneRepository(dbContext);
        Issues = new IssueRepository(dbContext);
    }

    public async Task<int> SaveChangesAsync()
    {
        await PublishDomainEvents();

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

    private async Task PublishDomainEvents()
    {
        var entitiesWithDomainEvents = _dbContext.ChangeTracker
            .Entries()
            .Where(entityEntry => entityEntry.Entity is IDomainEventProvider)
            .ToList();

        var domainEvents = entitiesWithDomainEvents
            .SelectMany(entityEntry => ((IDomainEventProvider)entityEntry.Entity).DomainEvents())
            .ToList();

        foreach (var entityEntry in entitiesWithDomainEvents)
            ((IDomainEventProvider)entityEntry.Entity).ClearDomainEvents();

        Console.WriteLine($"** About to publish {domainEvents.Count} domain event(s)!");

        IEnumerable<Task> tasks = domainEvents.Select(domainEvent => _publisher.Publish(domainEvent));

        await Task.WhenAll(tasks);
    }
}