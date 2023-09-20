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

internal sealed record GetAllIssuesQuery(
    Guid LaneId
) : IRequest<ErrorOr<IssuesResult>>;

[ApiExplorerSettings(GroupName = "Issues")]
public class GetAllIssuesController : ApiController
{
    private readonly IMediator _mediator;

    public GetAllIssuesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("/lanes/{laneId:guid}/issues")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IssuesResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetAllIssues(Guid laneId)
    {
        var getAllIssuesQuery = new GetAllIssuesQuery(laneId);
        var issuesResult = await _mediator.Send(getAllIssuesQuery);

        return issuesResult.Match(
            result => Ok(result.ToResponse()),
            errors => Problem(errors)
        );
    }
}

internal sealed class GetAllIssuesHandler : IRequestHandler<GetAllIssuesQuery, ErrorOr<IssuesResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;

    public GetAllIssuesHandler(IUnitOfWork unitOfWork, IAuthService authService)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
    }

    public async Task<ErrorOr<IssuesResult>> Handle(
        GetAllIssuesQuery getAllIssuesQuery,
        CancellationToken cancellationToken
    )
    {
        if (LaneId.Create(getAllIssuesQuery.LaneId) is not { } laneId)
            return Errors.EntityId.Invalid;

        if (await _authService.GetCurrentUserAsync() is not { } user)
            return Errors.Users.InvalidCredentials;

        if (await _unitOfWork.Lanes.GetByIdAsync(laneId) is not { } lane)
            return Errors.Lanes.NotFound;

        if (lane.User.Id != user.Id)
            return Errors.Users.InvalidCredentials;

        return new IssuesResult(lane.Issues);
    }
}
