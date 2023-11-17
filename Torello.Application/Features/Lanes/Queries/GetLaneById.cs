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

public sealed record GetLaneByIdQuery(Guid Id) : IRequest<ErrorOr<LaneResult>>;

[ApiExplorerSettings(GroupName = "Lanes")]
public sealed class GetLaneByIdController(ISender mediator) : ApiController
{
    [HttpGet("/lanes/{laneId:guid}", Name = nameof(GetLaneById))]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(LaneResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetLaneById(Guid laneId)
    {
        var getLaneByIdQuery = new GetLaneByIdQuery(laneId);
        var laneResult = await mediator.Send(getLaneByIdQuery);

        return laneResult.Match(
            result => Ok(result.ToResponse()),
            Problem
        );
    }
}

internal sealed class GetLaneByIdHandler(IUnitOfWork unitOfWork, IAuthService authService) : IRequestHandler<GetLaneByIdQuery, ErrorOr<LaneResult>>
{
    public async Task<ErrorOr<LaneResult>> Handle(GetLaneByIdQuery getLaneByIdQuery, CancellationToken cancellationToken)
    {
        if (LaneId.Create(getLaneByIdQuery.Id) is not { } laneId)
            return Errors.EntityId.Invalid;

        if (await authService.GetCurrentUserAsync() is not { } user)
            return Errors.Users.InvalidCredentials;

        if (await unitOfWork.Lanes.GetByIdAsync(laneId) is not { } lane)
            return Errors.Lanes.NotFound;

        if (lane.User.Id != user.Id)
            return Errors.Users.InvalidCredentials;

        return new LaneResult(lane);
    }
}
