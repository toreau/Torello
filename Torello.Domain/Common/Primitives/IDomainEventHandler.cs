using MediatR;

namespace Torello.Domain.Common.Primitives;

public interface IDomainEventHandler<in TDomainEvent> : INotificationHandler<TDomainEvent>
    where TDomainEvent : IDomainEvent;
