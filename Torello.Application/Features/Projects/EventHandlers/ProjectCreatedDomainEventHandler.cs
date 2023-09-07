using Torello.Domain.Common.Primitives;
using Torello.Domain.Projects;

namespace Torello.Application.Features.Projects.EventHandlers;

internal sealed class ProjectCreatedDomainEventHandler : IDomainEventHandler<ProjectCreatedDomainEvent>
{
    public ProjectCreatedDomainEventHandler()
    {
    }

    public Task Handle(
        ProjectCreatedDomainEvent notification,
        CancellationToken cancellationToken
    )
    {
        Console.WriteLine($"** Project '{notification.Project.Title}' was created! **");
        return Task.CompletedTask;
    }
}