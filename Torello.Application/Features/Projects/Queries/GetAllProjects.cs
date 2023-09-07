using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Torello.Application.Common;
using Torello.Application.Common.Interfaces.Persistence;
using Torello.Domain.Projects;

namespace Torello.Application.Features.Projects.Queries;

internal sealed record GetAllProjectsQuery : IRequest<ErrorOr<GetAllProjectsResult>>;

[ApiExplorerSettings(GroupName = "Projects")]
public sealed class GetAllProjectsController : ApiController
{
    private readonly IMediator _mediator;

    public GetAllProjectsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("/projects", Name = "GetAllProjects")]
    public async Task<IActionResult> GetAll()
    {
        var getAllProjectsQuery = new GetAllProjectsQuery();
        var getAllProjectsResult = await _mediator.Send(getAllProjectsQuery);

        return getAllProjectsResult.Match(
            result => Ok(result.ToResponse()),
            errors => Problem(errors)
        );
    }
}

internal sealed class GetAllProjectsHandler : IRequestHandler<GetAllProjectsQuery, ErrorOr<GetAllProjectsResult>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllProjectsHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<GetAllProjectsResult>> Handle(
        GetAllProjectsQuery getAllProjectsQuery,
        CancellationToken cancellationToken)
    {
        var projects = await _unitOfWork.Projects.GetAllAsync();

        return new GetAllProjectsResult(projects);
    }
}

internal sealed record GetAllProjectsResult(
    IEnumerable<Project> Projects
)
{
    public GetAllProjectsResponse ToResponse()
        => new GetAllProjectsResponse(Projects.Select(p => new GetProjectResponse(
            p.Id.Value.ToString(),
            p.Name, p.CreatedAt.ToString("s")
        )).ToList());
}

internal sealed record GetAllProjectsResponse(
    List<GetProjectResponse> Projects
);

internal sealed record GetProjectResponse(
    string Id,
    string Name,
    string CreatedAt
);
