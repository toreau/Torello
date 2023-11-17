using Torello.Domain.Common.Interfaces;
using Torello.Domain.Projects;

namespace Torello.Application.Features.Projects.EventHandlers;

internal sealed class ProjectCreatedDomainEventHandler : IDomainEventHandler<ProjectCreatedDomainEvent>
{
    public async Task Handle(ProjectCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        Console.WriteLine($"** Project '{notification.Project.Title}' was created! **");
    }
}
