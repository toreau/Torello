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

internal sealed record GetAllLanesQuery(Guid BoardId) : IRequest<ErrorOr<LanesResult>>;

[ApiExplorerSettings(GroupName = "Lanes")]
public class GetAllLanesController(ISender mediator) : ApiController
{
    [HttpGet("/boards/{boardId:guid}/lanes")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(BoardsResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetAllLanes(Guid boardId)
    {
        var getAllLanesQuery = new GetAllLanesQuery(boardId);
        var lanesResult = await mediator.Send(getAllLanesQuery);

        return lanesResult.Match(
            result => Ok(result.ToResponse()),
            Problem
        );
    }
}

internal sealed class GetAllLanesHandler(IUnitOfWork unitOfWork, IAuthService authService) : IRequestHandler<GetAllLanesQuery, ErrorOr<LanesResult>>
{
    public async Task<ErrorOr<LanesResult>> Handle(GetAllLanesQuery getAllLanesQuery, CancellationToken cancellationToken)
    {
        if (BoardId.Create(getAllLanesQuery.BoardId) is not { } boardId)
            return Errors.EntityId.Invalid;

        if (await authService.GetCurrentUserAsync() is not { } user)
            return Errors.Users.InvalidCredentials;

        if (await unitOfWork.Boards.GetByIdAsync(boardId) is not { } board)
            return Errors.Boards.NotFound;

        if (board.Owner.Id != user.Id)
            return Errors.Users.InvalidCredentials;

        return new LanesResult(board.Lanes);
    }
}
