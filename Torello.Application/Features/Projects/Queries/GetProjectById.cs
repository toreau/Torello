using System.Net.Mime;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Torello.Application.Common;
using Torello.Application.Common.Interfaces.Persistence;
using Torello.Contracts;
using Torello.Domain.Common.Errors;
using Torello.Domain.Projects;

namespace Torello.Application.Features.Projects.Queries;

public sealed record GetProjectByIdQuery(
    Guid Id
) : IRequest<ErrorOr<ProjectResult>>;

[ApiExplorerSettings(GroupName = "Projects")]
public sealed class GetProjectByIdController : ApiController
{
    private readonly IMediator _mediator;

    public GetProjectByIdController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("/projects/{projectId:guid}", Name = nameof(GetProjectById))]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ProjectResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetProjectById(Guid projectId)
    {
        var getProjectByIdQuery = new GetProjectByIdQuery(projectId);
        var getProjectByIdResult = await _mediator.Send(getProjectByIdQuery);

        return getProjectByIdResult.Match(
            result => Ok(result.ToResponse()),
            errors => Problem(errors)
        );
    }
}

internal sealed class GetProjectByIdHandler : IRequestHandler<GetProjectByIdQuery, ErrorOr<ProjectResult>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProjectByIdHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<ProjectResult>> Handle(
        GetProjectByIdQuery getProjectByIdQuery,
        CancellationToken cancellationToken
    )
    {
        if (ProjectId.Create(getProjectByIdQuery.Id) is not { } projectId)
            return Errors.EntityId.Invalid;

        if (await _unitOfWork.Projects.GetByIdAsync(projectId) is not { } project)
            return Errors.Projects.NotFound;

        return new ProjectResult(project);
    }
}
