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
public sealed class GetBoardByIdController : ApiController
{
    private readonly IMediator _mediator;

    public GetBoardByIdController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("/boards/{boardId:guid}", Name = nameof(GetBoardById))]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(BoardResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetBoardById(Guid boardId)
    {
        var getBoardByIdQuery = new GetBoardByIdQuery(boardId);
        var boardResult = await _mediator.Send(getBoardByIdQuery);

        return boardResult.Match(
            result => Ok(result.ToResponse()),
            errors => Problem(errors)
        );
    }
}

internal sealed class GetBoardByIdHandler : IRequestHandler<GetBoardByIdQuery, ErrorOr<BoardResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;

    public GetBoardByIdHandler(IUnitOfWork unitOfWork, IAuthService authService)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
    }

    public async Task<ErrorOr<BoardResult>> Handle(
        GetBoardByIdQuery getBoardByIdQuery,
        CancellationToken cancellationToken
    )
    {
        if (BoardId.Create(getBoardByIdQuery.Id) is not { } boardId)
            return Errors.EntityId.Invalid;

        if (await _authService.GetCurrentUserAsync() is not { } user)
            return Errors.Users.InvalidCredentials;

        if (await _unitOfWork.Boards.GetByIdAsync(boardId) is not { } board)
            return Errors.Boards.NotFound;

        return new BoardResult(board);
    }
}
