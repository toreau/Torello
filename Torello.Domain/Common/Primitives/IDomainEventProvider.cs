namespace Torello.Domain.Common.Primitives;

public interface IDomainEventProvider
{
    IReadOnlyList<IDomainEvent> DomainEvents();
    void AddDomainEvent(IDomainEvent domainEvent);
    void ClearDomainEvents();
}