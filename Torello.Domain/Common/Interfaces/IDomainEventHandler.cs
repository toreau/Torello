using MediatR;

namespace Torello.Domain.Common.Interfaces;

public interface IDomainEventHandler<in TDomainEvent> : INotificationHandler<TDomainEvent>
    where TDomainEvent : IDomainEvent;
