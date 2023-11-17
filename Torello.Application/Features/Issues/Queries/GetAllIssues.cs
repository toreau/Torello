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

namespace Torello.Application.Features.Issues.Queries;

internal sealed record GetAllIssuesQuery(Guid LaneId) : IRequest<ErrorOr<IssuesResult>>;

[ApiExplorerSettings(GroupName = "Issues")]
public class GetAllIssuesController(ISender mediator) : ApiController
{
    [HttpGet("/lanes/{laneId:guid}/issues")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IssuesResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetAllIssues(Guid laneId)
    {
        var getAllIssuesQuery = new GetAllIssuesQuery(laneId);
        var issuesResult = await mediator.Send(getAllIssuesQuery);

        return issuesResult.Match(
            result => Ok(result.ToResponse()),
            Problem
        );
    }
}

internal sealed class GetAllIssuesHandler(IUnitOfWork unitOfWork, IAuthService authService) : IRequestHandler<GetAllIssuesQuery, ErrorOr<IssuesResult>>
{
    public async Task<ErrorOr<IssuesResult>> Handle(GetAllIssuesQuery getAllIssuesQuery, CancellationToken cancellationToken)
    {
        if (LaneId.Create(getAllIssuesQuery.LaneId) is not { } laneId)
            return Errors.EntityId.Invalid;

        if (await authService.GetCurrentUserAsync() is not { } user)
            return Errors.Users.InvalidCredentials;

        if (await unitOfWork.Lanes.GetByIdAsync(laneId) is not { } lane)
            return Errors.Lanes.NotFound;

        if (lane.User.Id != user.Id)
            return Errors.Users.InvalidCredentials;

        return new IssuesResult(lane.Issues);
    }
}
