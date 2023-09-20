using System.Net.Mime;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Torello.Application.Common;
using Torello.Application.Common.Interfaces;
using Torello.Application.Common.Interfaces.Persistence;
using Torello.Application.Features.Lanes.Queries;
using Torello.Contracts;
using Torello.Domain.Boards;
using Torello.Domain.Common.Errors;
using Torello.Domain.Lanes;

namespace Torello.Application.Features.Lanes.Commands;

public sealed record UpsertLaneRequest(
    string Title
)
{
    public UpsertLaneCommand ToCommand(LaneId? laneId)
        => new UpsertLaneCommand(
            Title,
            laneId,
            null
        );

    public UpsertLaneCommand ToCommand(BoardId? boardId)
        => new UpsertLaneCommand(
            Title,
            null,
            boardId
        );
}

public sealed record UpsertLaneCommand(
    string Title,
    LaneId? LaneId,
    BoardId? BoardId
) : IRequest<ErrorOr<LaneResult>>;

[ApiExplorerSettings(GroupName = "Lanes")]
public sealed class UpsertLaneController : ApiController
{
    private readonly IMediator _mediator;

    public UpsertLaneController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("/boards/{boardId:guid}/lanes", Name = nameof(CreateLane))]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(LaneResponse), 201)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CreateLane(Guid boardId, UpsertLaneRequest upsertLaneRequest)
    {
        return await UpsertLane(upsertLaneRequest.ToCommand(BoardId.Create(boardId)));
    }

    [HttpPut("/lanes/{laneId:guid}", Name = nameof(UpdateLane))]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(LaneResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateLane(Guid laneId, UpsertLaneRequest upsertLaneRequest)
    {
        return await UpsertLane(upsertLaneRequest.ToCommand(LaneId.Create(laneId)));
    }

    private async Task<IActionResult> UpsertLane(UpsertLaneCommand upsertLaneCommand)
    {
        var upsertLaneResult = await _mediator.Send(upsertLaneCommand);

        return upsertLaneResult.Match(
            result =>
            {
                var response = result.ToResponse();

                return upsertLaneCommand.LaneId is null
                    ? CreatedAtRoute(
                        nameof(GetLaneByIdController.GetLaneById),
                        new { boardId = response.Id },
                        response)
                    : Ok(response);
            },
            errors => Problem(errors)
        );
    }
}

public sealed class UpsertLaneValidator : AbstractValidator<UpsertLaneCommand>
{
    private const byte MinLaneTitleLength = 2;
    private const byte MaxLaneTitleLength = 32;

    public UpsertLaneValidator()
    {
        RuleFor(x => x.Title)
            .MinimumLength(MinLaneTitleLength).WithMessage($"The lane title must be minimum {MinLaneTitleLength} characters long!")
            .MaximumLength(MaxLaneTitleLength).WithMessage($"The lane title must be maximum {MaxLaneTitleLength} characters long!");
    }
}

internal sealed class UpsertLaneHandler : IRequestHandler<UpsertLaneCommand, ErrorOr<LaneResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;

    public UpsertLaneHandler(IUnitOfWork unitOfWork, IAuthService authService)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
    }

    public async Task<ErrorOr<LaneResult>> Handle(
        UpsertLaneCommand upsertLaneCommand,
        CancellationToken cancellationToken
    )
    {
        if (await _authService.GetCurrentUserAsync() is not { } user)
            return Errors.Users.InvalidCredentials;

        Lane? lane;

        // Update an existing lane?
        if (upsertLaneCommand.LaneId is not null)
        {
            // Does the lane exist?
            lane = await _unitOfWork.Lanes.GetByIdAsync(upsertLaneCommand.LaneId);
            if (lane is null)
                return Errors.Lanes.NotFound;

            // Is the lane's owner the same as the one logged in?
            if (lane.UserId != user.Id)
                return Errors.Users.InvalidCredentials;

            lane.Update(upsertLaneCommand.Title);
        }
        else
        {
            // Does the board exist?
            if (await _unitOfWork.Boards.GetByIdAsync(upsertLaneCommand.BoardId!) is not { } board)
                return Errors.Boards.NotFound;

            lane = Lane.Create(upsertLaneCommand.Title);

            board.AddLane(lane);
        }

        await _unitOfWork.SaveChangesAsync();

        return new LaneResult(lane);
    }
}
