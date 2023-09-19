using System.Net.Mime;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Torello.Application.Common;
using Torello.Application.Common.Interfaces;
using Torello.Application.Common.Interfaces.Persistence;
using Torello.Application.Features.Boards.Queries;
using Torello.Contracts;
using Torello.Domain.Boards;
using Torello.Domain.Common.Errors;
using Torello.Domain.Projects;

namespace Torello.Application.Features.Boards.Commands;

public sealed record UpsertBoardRequest(
    string Title
)
{
    public UpsertBoardCommand ToCommand(BoardId? boardId)
        => new UpsertBoardCommand(
            Title,
            boardId,
            null
        );

    public UpsertBoardCommand ToCommand(ProjectId? projectId)
        => new UpsertBoardCommand(
            Title,
            null,
            projectId
        );
}

public sealed record UpsertBoardCommand(
    string Title,
    BoardId? BoardId,
    ProjectId? ProjectId
) : IRequest<ErrorOr<BoardResult>>;

[ApiExplorerSettings(GroupName = "Boards")]
public sealed class UpsertBoardController : ApiController
{
    private readonly IMediator _mediator;

    public UpsertBoardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [ApiExplorerSettings(GroupName = "Projects → Boards")]
    [HttpPost("/projects/{projectId:guid}/boards", Name = nameof(CreateBoard))]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(BoardResponse), 201)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CreateBoard(Guid projectId, UpsertBoardRequest upsertBoardRequest)
    {
        return await UpsertBoard(upsertBoardRequest.ToCommand(ProjectId.Create(projectId)));
    }

    [HttpPut("/boards/{boardId:guid}", Name = nameof(UpdateBoard))]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(BoardResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateBoard(Guid boardId, UpsertBoardRequest upsertBoardRequest)
    {
        return await UpsertBoard(upsertBoardRequest.ToCommand(BoardId.Create(boardId)));
    }

    private async Task<IActionResult> UpsertBoard(UpsertBoardCommand upsertBoardCommand)
    {
        var upsertBoardResult = await _mediator.Send(upsertBoardCommand);

        return upsertBoardResult.Match(
            result =>
            {
                var response = result.ToResponse();

                return upsertBoardCommand.BoardId is null
                    ? CreatedAtRoute(
                        nameof(GetBoardByIdController.GetBoardById),
                        new { boardId = response.Id },
                        response)
                    : Ok(response);
            },
            errors => Problem(errors)
        );

    }
}

internal sealed class UpsertBoardHandler : IRequestHandler<UpsertBoardCommand, ErrorOr<BoardResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;

    public UpsertBoardHandler(IUnitOfWork unitOfWork, IAuthService authService)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
    }

    public async Task<ErrorOr<BoardResult>> Handle(
        UpsertBoardCommand upsertBoardCommand,
        CancellationToken cancellationToken
    )
    {
        if (await _authService.GetCurrentUserAsync() is not { } user)
            return Errors.Users.InvalidCredentials;

        Board? board;

        // Update an existing board?
        if (upsertBoardCommand.BoardId is not null)
        {
            // Does the board exist?
            board = await _unitOfWork.Boards.GetByIdAsync(upsertBoardCommand.BoardId);
            if (board is null)
                return Errors.Boards.NotFound;

            // Is the board's owner the same as the one logged in?
            if (board.UserId != user.Id)
                return Errors.Users.InvalidCredentials;

            board.Update(upsertBoardCommand.Title);
        }
        // Create a new board
        else
        {
            // Does the project exist?
            if (await _unitOfWork.Projects.GetByIdAsync(upsertBoardCommand.ProjectId!) is not { } project)
                return Errors.Projects.NotFound;

            board = Board.Create(upsertBoardCommand.Title);

            project.AddBoard(board);
        }

        await _unitOfWork.SaveChangesAsync();

        return new BoardResult(board);
    }
}