using System.Net.Mime;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Torello.Application.Common;
using Torello.Application.Common.Interfaces;
using Torello.Application.Common.Interfaces.Persistence;
using Torello.Contracts;
using Torello.Domain.Boards;
using Torello.Domain.Common.Errors;

namespace Torello.Application.Features.Lanes.Queries;

internal sealed record GetAllLanesQuery(
    Guid BoardId
) : IRequest<ErrorOr<LanesResult>>;

[ApiExplorerSettings(GroupName = "Lanes")]
public class GetAllLanesController : ApiController
{
    private readonly IMediator _mediator;

    public GetAllLanesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("/boards/{boardId:guid}/lanes")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(BoardsResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetAllLanes(Guid boardId)
    {
        var getAllLanesQuery = new GetAllLanesQuery(boardId);
        var lanesResult = await _mediator.Send(getAllLanesQuery);

        return lanesResult.Match(
            result => Ok(result.ToResponse()),
            errors => Problem(errors)
        );
    }
}

internal sealed class GetAllLanesHandler : IRequestHandler<GetAllLanesQuery, ErrorOr<LanesResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;

    public GetAllLanesHandler(IUnitOfWork unitOfWork, IAuthService authService)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
    }

    public async Task<ErrorOr<LanesResult>> Handle(
        GetAllLanesQuery getAllLanesQuery,
        CancellationToken cancellationToken
    )
    {
        if (BoardId.Create(getAllLanesQuery.BoardId) is not { } boardId)
            return Errors.EntityId.Invalid;

        if (await _authService.GetCurrentUserAsync() is not { } user)
            return Errors.Users.InvalidCredentials;

        if (await _unitOfWork.Boards.GetByIdAsync(boardId) is not { } board)
            return Errors.Boards.NotFound;

        if (board.UserId != user.Id)
            return Errors.Users.InvalidCredentials;

        return new LanesResult(board.Lanes);
    }
}
