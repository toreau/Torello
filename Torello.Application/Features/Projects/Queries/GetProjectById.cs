using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Torello.Application.Common;
using Torello.Application.Common.Interfaces.Persistence;
using Torello.Domain.Boards;
using Torello.Domain.Common.Errors;
using Torello.Domain.Projects;

namespace Torello.Application.Features.Projects.Queries;

public sealed record GetProjectByIdQuery(
    string Id
) : IRequest<ErrorOr<GetProjectByIdResult>>;

[ApiExplorerSettings(GroupName = "Projects")]
public sealed class GetProjectByIdController : ApiController
{
    private readonly IMediator _mediator;

    public GetProjectByIdController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("/projects/{id}", Name = nameof(GetProjectById))]
    public async Task<IActionResult> GetProjectById(string id)
    {
        var getProjectByIdQuery = new GetProjectByIdQuery(id);
        var getProjectByIdResult = await _mediator.Send(getProjectByIdQuery);

        return getProjectByIdResult.Match(
            result => Ok(result.ToResponse()),
            errors => Problem(errors)
        );
    }
}

internal sealed class GetProjectByIdHandler : IRequestHandler<GetProjectByIdQuery, ErrorOr<GetProjectByIdResult>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProjectByIdHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<GetProjectByIdResult>> Handle(
        GetProjectByIdQuery getProjectByIdQuery,
        CancellationToken cancellationToken
    )
    {
        if (ProjectId.Create(getProjectByIdQuery.Id) is not {} projectId)
            return Errors.EntityId.Invalid;

        if (await _unitOfWork.Projects.GetByIdAsync(projectId) is not { } project)
            return Errors.Projects.NotFound;

        return new GetProjectByIdResult(project);
    }
}

internal sealed record GetProjectByIdResult(
    Project Project
)
{
    public GetProjectByIdResponse ToResponse()
        => new GetProjectByIdResponse(
            Project.Id.Value.ToString(),
            Project.Title,
            Project.Description,
            Project.CreatedAt.ToString("s"),
            Project.Boards.Select(
                board => new GetBoardResult(board).ToResponse()).ToList());
}

internal sealed record GetProjectByIdResponse(
    string Id,
    string Title,
    string Description,
    string CreatedAt,
    List<GetBoardResponse> Boards
);

// TODO: Move this to 'Board' responsibility
internal sealed record GetBoardResult(
    Board Board
)
{
    public GetBoardResponse ToResponse()
        => new GetBoardResponse(
            Board.Id.Value.ToString(),
            Board.Title,
            Board.CreatedAt.ToString("s"));
}

internal sealed record GetBoardResponse(
    string Id,
    string Title,
    string CreatedAt
);
