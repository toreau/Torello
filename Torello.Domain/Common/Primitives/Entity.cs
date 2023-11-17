using Torello.Domain.Common.Interfaces;

namespace Torello.Domain.Common.Primitives;

public abstract class Entity<TEntityId>(TEntityId id) : IDomainEventProvider
{
    public TEntityId Id { get; protected set; } = id;

    private readonly List<IDomainEvent> _domainEvents = new();

    public IReadOnlyList<IDomainEvent> DomainEvents() => _domainEvents.AsReadOnly();

    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
}
