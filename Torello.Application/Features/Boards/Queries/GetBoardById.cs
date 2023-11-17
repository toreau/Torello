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

namespace Torello.Application.Features.Boards.Queries;

public sealed record GetBoardByIdQuery(
    Guid Id
) : IRequest<ErrorOr<BoardResult>>;

[ApiExplorerSettings(GroupName = "Boards")]
public sealed class GetBoardByIdController(ISender mediator) : ApiController
{
    [HttpGet("/boards/{boardId:guid}", Name = nameof(GetBoardById))]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(BoardResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetBoardById(Guid boardId)
    {
        var getBoardByIdQuery = new GetBoardByIdQuery(boardId);
        var boardResult = await mediator.Send(getBoardByIdQuery);

        return boardResult.Match(
            result => Ok(result.ToResponse()),
            errors => Problem(errors)
        );
    }
}

internal sealed class GetBoardByIdHandler(IUnitOfWork unitOfWork, IAuthService authService) : IRequestHandler<GetBoardByIdQuery, ErrorOr<BoardResult>>
{
    public async Task<ErrorOr<BoardResult>> Handle(
        GetBoardByIdQuery getBoardByIdQuery,
        CancellationToken cancellationToken
    )
    {
        if (BoardId.Create(getBoardByIdQuery.Id) is not { } boardId)
            return Errors.EntityId.Invalid;

        if (await authService.GetCurrentUserAsync() is not { } user)
            return Errors.Users.InvalidCredentials;

        if (await unitOfWork.Boards.GetByIdAsync(boardId) is not { } board)
            return Errors.Boards.NotFound;

        if (board.User.Id != user.Id)
            return Errors.Users.InvalidCredentials;

        return new BoardResult(board);
    }
}
