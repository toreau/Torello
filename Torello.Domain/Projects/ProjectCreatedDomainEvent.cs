using Torello.Domain.Common.Interfaces;
using Torello.Domain.Common.Primitives;

namespace Torello.Domain.Projects;

public sealed class ProjectCreatedDomainEvent : IDomainEvent
{
    public Project Project { get; }

    internal ProjectCreatedDomainEvent(Project project)
    {
        Project = project;
    }
}
