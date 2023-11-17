using System.Net.Mime;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Torello.Application.Common;
using Torello.Application.Common.Interfaces;
using Torello.Application.Common.Interfaces.Persistence;
using Torello.Contracts;
using Torello.Domain.Common.Errors;
using Torello.Domain.Projects;

namespace Torello.Application.Features.Projects.Queries;

public sealed record GetProjectByIdQuery(Guid ProjectId) : IRequest<ErrorOr<ProjectResult>>;

[ApiExplorerSettings(GroupName = "Projects")]
public sealed class GetProjectByIdController(ISender mediator) : ApiController
{
    [HttpGet("/projects/{projectId:guid}", Name = nameof(GetProjectById))]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ProjectResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetProjectById(Guid projectId)
    {
        var getProjectByIdQuery = new GetProjectByIdQuery(projectId);
        var getProjectByIdResult = await mediator.Send(getProjectByIdQuery);

        return getProjectByIdResult.Match(
            result => Ok(result.ToResponse()),
            errors => Problem(errors)
        );
    }
}

internal sealed class GetProjectByIdHandler(
    IUnitOfWork unitOfWork,
    IAuthService authService) : IRequestHandler<GetProjectByIdQuery, ErrorOr<ProjectResult>>
{
    public async Task<ErrorOr<ProjectResult>> Handle(GetProjectByIdQuery getProjectByIdQuery, CancellationToken cancellationToken)
    {
        if (ProjectId.Create(getProjectByIdQuery.ProjectId) is not { } projectId)
            return Errors.EntityId.Invalid;

        if (await authService.GetCurrentUserAsync() is not { } user)
            return Errors.Users.InvalidCredentials;

        if (await unitOfWork.Projects.GetByIdAsync(projectId) is not { } project)
            return Errors.Projects.NotFound;

        if (project.User.Id != user.Id)
            return Errors.Users.InvalidCredentials;

        return new ProjectResult(project);
    }
}
