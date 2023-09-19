using System.Net.Mime;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Torello.Application.Common;
using Torello.Application.Common.Interfaces;
using Torello.Application.Common.Interfaces.Persistence;
using Torello.Contracts;
using Torello.Domain.Common.Errors;
using Torello.Domain.Lanes;

namespace Torello.Application.Features.Lanes.Queries;

public sealed record GetLaneByIdQuery(
    Guid Id
) : IRequest<ErrorOr<LaneResult>>;

[ApiExplorerSettings(GroupName = "Lanes")]
public sealed class GetLaneByIdController : ApiController
{
    private readonly IMediator _mediator;

    public GetLaneByIdController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("/lanes/{laneId:guid}", Name = nameof(GetLaneById))]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(LaneResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetLaneById(Guid laneId)
    {
        var getLaneByIdQuery = new GetLaneByIdQuery(laneId);
        var laneResult = await _mediator.Send(getLaneByIdQuery);

        return laneResult.Match(
            result => Ok(result.ToResponse()),
            errors => Problem(errors)
        );
    }
}

internal sealed class GetLaneByIdHandler : IRequestHandler<GetLaneByIdQuery, ErrorOr<LaneResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;

    public GetLaneByIdHandler(IUnitOfWork unitOfWork, IAuthService authService)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
    }

    public async Task<ErrorOr<LaneResult>> Handle(
        GetLaneByIdQuery getLaneByIdQuery,
        CancellationToken cancellationToken)
    {
        if (LaneId.Create(getLaneByIdQuery.Id) is not { } laneId)
            return Errors.EntityId.Invalid;

        if (await _authService.GetCurrentUserAsync() is not { } user)
            return Errors.Users.InvalidCredentials;

        if (await _unitOfWork.Lanes.GetByIdAsync(laneId) is not { } lane)
            return Errors.Lanes.NotFound;

        if (lane.UserId != user.Id)
            return Errors.Users.InvalidCredentials;

        return new LaneResult(lane);
    }
}