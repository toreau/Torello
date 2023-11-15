using MediatR;
using Torello.Application.Common.Interfaces.Persistence;
using Torello.Domain.Common.Primitives;
using Torello.Infrastructure.Persistence.Repositories;
using static System.GC;

namespace Torello.Infrastructure.Persistence;

public class UnitOfWork(TorelloDbContext dbContext, IPublisher publisher) : IUnitOfWork
{
    private bool _isDisposed;

    public IUserRepository Users { get; } = new UserRepository(dbContext);
    public IProjectRepository Projects { get; } = new ProjectRepository(dbContext);
    public IBoardRepository Boards { get; } = new BoardRepository(dbContext);
    public ILaneRepository Lanes { get; } = new LaneRepository(dbContext);
    public IIssueRepository Issues { get; } = new IssueRepository(dbContext);

    public async Task<int> SaveChangesAsync()
    {
        await PublishDomainEvents();

        return await dbContext.SaveChangesAsync();
    }

    public void Dispose()
    {
        Dispose(true);
        SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_isDisposed)
            return;

        if (disposing)
            dbContext.Dispose();

        _isDisposed = true;
    }

    private async Task PublishDomainEvents()
    {
        // Create a list of all the entities that may have domain events
        var entitiesWithDomainEvents = dbContext.ChangeTracker
            .Entries()
            .Where(entityEntry => entityEntry.Entity is IDomainEventProvider)
            .ToList();

        // Create a list of all the domain events
        var domainEvents = entitiesWithDomainEvents
            .SelectMany(entityEntry => ((IDomainEventProvider)entityEntry.Entity).DomainEvents())
            .ToList();

        // Clear the domain events for each entity
        foreach (var entityEntry in entitiesWithDomainEvents)
            ((IDomainEventProvider)entityEntry.Entity).ClearDomainEvents();

        Console.WriteLine($"** About to publish {domainEvents.Count} domain event(s)!");

        // Publish the domain events and await them
        IEnumerable<Task> tasks = domainEvents.Select(domainEvent => publisher.Publish(domainEvent));

        await Task.WhenAll(tasks);
    }
}
