namespace Torello.Domain.Common.Interfaces;

public interface IDomainEventProvider
{
    IReadOnlyList<IDomainEvent> DomainEvents();
    void AddDomainEvent(IDomainEvent domainEvent);
    void ClearDomainEvents();
}