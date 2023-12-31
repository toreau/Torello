using System.Net.Mime;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Torello.Application.Common;
using Torello.Application.Common.Interfaces;
using Torello.Contracts;
using Torello.Domain.Common.Errors;

namespace Torello.Application.Features.Projects.Queries;

internal sealed record GetAllProjectsQuery : IRequest<ErrorOr<ProjectsResult>>;

[ApiExplorerSettings(GroupName = "Projects")]
public sealed class GetAllProjectsController(ISender mediator) : ApiController
{
    [HttpGet("/projects", Name = nameof(GetAllProjects))]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ProjectsResponse), 200)]
    public async Task<IActionResult> GetAllProjects()
    {
        var getAllProjectsQuery = new GetAllProjectsQuery();
        var projectsResult = await mediator.Send(getAllProjectsQuery);

        return projectsResult.Match(
            result => Ok(result.ToResponse()),
            Problem
        );
    }
}

internal sealed class GetAllProjectsHandler(IAuthService authService) : IRequestHandler<GetAllProjectsQuery, ErrorOr<ProjectsResult>>
{
    public async Task<ErrorOr<ProjectsResult>> Handle(GetAllProjectsQuery getAllProjectsQuery, CancellationToken cancellationToken)
    {
        if (await authService.GetCurrentUserAsync() is not { } user)
            return Errors.Users.InvalidCredentials;

        return new ProjectsResult(user.Projects());
    }
}
