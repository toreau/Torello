using System.Net.Mime;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Torello.Application.Common;
using Torello.Application.Common.Interfaces.Persistence;
using Torello.Contracts;

namespace Torello.Application.Features.Projects.Queries;

internal sealed record GetAllProjectsQuery(
) : IRequest<ErrorOr<ProjectsResult>>;

[ApiExplorerSettings(GroupName = "Projects")]
public sealed class GetAllProjectsController : ApiController
{
    private readonly IMediator _mediator;

    public GetAllProjectsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("/projects", Name = nameof(GetAllProjects))]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ProjectsResponse), 200)]
    public async Task<IActionResult> GetAllProjects()
    {
        var getAllProjectsQuery = new GetAllProjectsQuery();
        var projectsResult = await _mediator.Send(getAllProjectsQuery);

        return projectsResult.Match(
            result => Ok(result.ToResponse()),
            errors => Problem(errors)
        );
    }
}

internal sealed class GetAllProjectsHandler : IRequestHandler<GetAllProjectsQuery, ErrorOr<ProjectsResult>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllProjectsHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<ProjectsResult>> Handle(
        GetAllProjectsQuery getAllProjectsQuery,
        CancellationToken cancellationToken)
    {
        var projects = await _unitOfWork.Projects.GetAllAsync();

        return new ProjectsResult(projects);
    }
}
