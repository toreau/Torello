using Torello.Domain.Common.Interfaces;

namespace Torello.Domain.Projects;

public sealed class ProjectCreatedDomainEvent : IDomainEvent
{
    public Project Project { get; }

    internal ProjectCreatedDomainEvent(Project project)
    {
        Project = project;
    }
}
