using System.Net.Mime;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Torello.Application.Common;
using Torello.Application.Common.Interfaces;
using Torello.Application.Common.Interfaces.Persistence;
using Torello.Contracts;
using Torello.Domain.Common.Errors;
using Torello.Domain.Issues;

namespace Torello.Application.Features.Issues.Queries;

public sealed record GetIssueByIdQuery(Guid Id) : IRequest<ErrorOr<IssueResult>>;

[ApiExplorerSettings(GroupName = "Issues")]
public sealed class GetIssueByIdController(ISender mediator) : ApiController
{
    [HttpGet("/issues/{issueId:guid}", Name = nameof(GetIssueById))]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(LaneResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetIssueById(Guid issueId)
    {
        var getIssueByIdQuery = new GetIssueByIdQuery(issueId);
        var issueResult = await mediator.Send(getIssueByIdQuery);

        return issueResult.Match(
            result => Ok(result.ToResponse()),
            Problem
        );
    }
}

internal sealed class GetIssueByIdHandler(IUnitOfWork unitOfWork, IAuthService authService) : IRequestHandler<GetIssueByIdQuery, ErrorOr<IssueResult>>
{
    public async Task<ErrorOr<IssueResult>> Handle(GetIssueByIdQuery getIssueByIdQuery, CancellationToken cancellationToken)
    {
        if (IssueId.Create(getIssueByIdQuery.Id) is not { } issueId)
            return Errors.EntityId.Invalid;

        if (await authService.GetCurrentUserAsync() is not { } user)
            return Errors.Users.InvalidCredentials;

        if (await unitOfWork.Issues.GetByIdAsync(issueId) is not { } issue)
            return Errors.Issues.NotFound;

        if (issue.Owner.Id != user.Id)
            return Errors.Users.InvalidCredentials;

        return new IssueResult(issue);
    }
}
