namespace Torello.Domain.Common.Primitives;

public abstract class Entity<TEntityId> : IDomainEventProvider
{
    public TEntityId Id { get; protected set; }

    private readonly List<IDomainEvent> _domainEvents = new List<IDomainEvent>();

    public IReadOnlyList<IDomainEvent> DomainEvents() => _domainEvents.AsReadOnly();

    protected Entity(TEntityId id)
    {
        Id = id;
    }

    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
}
