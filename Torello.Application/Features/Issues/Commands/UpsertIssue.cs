using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Torello.Application.Common;
using Torello.Application.Common.Interfaces;
using Torello.Application.Common.Interfaces.Persistence;
using Torello.Application.Features.Issues.Queries;
using Torello.Contracts;
using Torello.Domain.Common.Errors;
using Torello.Domain.Issues;
using Torello.Domain.Lanes;

namespace Torello.Application.Features.Issues.Commands;

public sealed record UpsertIssueRequest(
    string Title,
    string Description
)
{
    public UpsertIssueCommand ToCommand(IssueId? issueId)
        => new UpsertIssueCommand(
            Title,
            Description,
            issueId,
            null
        );

    public UpsertIssueCommand ToCommand(LaneId? laneId)
        => new UpsertIssueCommand(
            Title,
            Description,
            null,
            laneId
        );
}

public sealed record UpsertIssueCommand(
    string Title,
    string Description,
    IssueId? IssueId,
    LaneId? LaneId
) : IRequest<ErrorOr<IssueResult>>;

[ApiExplorerSettings(GroupName = "Issues")]
public sealed class UpsertIssueController : ApiController
{
    private readonly IMediator _mediator;

    public UpsertIssueController(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IActionResult> CreateIssue(Guid laneId, UpsertIssueRequest upsertIssueRequest)
    {
        return await UpsertIssue(upsertIssueRequest.ToCommand(LaneId.Create(laneId)));
    }

    public async Task<IActionResult> UpdateIssue(Guid issueId, UpsertIssueRequest upsertIssueRequest)
    {
        return await UpsertIssue(upsertIssueRequest.ToCommand(IssueId.Create(issueId)));
    }

    private async Task<IActionResult> UpsertIssue(UpsertIssueCommand upsertIssueCommand)
    {
        var upsertIssueResult = await _mediator.Send(upsertIssueCommand);

        return upsertIssueResult.Match(
            result =>
            {
                var response = result.ToResponse();

                return upsertIssueCommand.LaneId is null
                    ? CreatedAtRoute(
                        nameof(GetIssueByIdController.GetIssueById),
                        new { issueId = response.Id },
                        response)
                    : Ok(response);
            },
            errors => Problem(errors)
        );
    }
}

public sealed class UpsertIssueValidator : AbstractValidator<UpsertIssueCommand>
{
    private const byte MinIssueTitleLength = 4;
    private const byte MaxIssueTitleLength = 32;

    public UpsertIssueValidator()
    {
        RuleFor(x => x.Title)
            .MinimumLength(MinIssueTitleLength).WithMessage($"The issue title must be minimum {MinIssueTitleLength} characters long!")
            .MaximumLength(MaxIssueTitleLength).WithMessage($"The issue title must be maximum {MaxIssueTitleLength} characters long!");
    }
}

internal sealed class UpsertIssueHandler : IRequestHandler<UpsertIssueCommand, ErrorOr<IssueResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;

    public UpsertIssueHandler(IUnitOfWork unitOfWork, IAuthService authService)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
    }

    public async Task<ErrorOr<IssueResult>> Handle(
        UpsertIssueCommand upsertIssueCommand,
        CancellationToken cancellationToken
    )
    {
        if (await _authService.GetCurrentUserAsync() is not { } user)
            return Errors.Users.InvalidCredentials;

        Issue? issue;

        // Update an existing issue?
        if (upsertIssueCommand.IssueId is not null)
        {
            // Does the issue exist?
            issue = await _unitOfWork.Issues.GetByIdAsync(upsertIssueCommand.IssueId);
            if (issue is null)
                return Errors.Issues.NotFound;

            // Is the issue's owner the same as the one logged in?
            if (issue.UserId != user.Id)
                return Errors.Users.InvalidCredentials;

            issue.Update(
                upsertIssueCommand.Title,
                upsertIssueCommand.Description
            );
        }
        // Create a new issue?
        else
        {
            // Does the lane exist?
            if (await _unitOfWork.Lanes.GetByIdAsync(upsertIssueCommand.LaneId) is not { } lane)
                return Errors.Lanes.NotFound;

            issue = Issue.Create(
                upsertIssueCommand.Title,
                upsertIssueCommand.Description,
                IssuePriority.Low
            );

            lane.AddIssue(issue);
        }

        await _unitOfWork.SaveChangesAsync();

        return new IssueResult(issue);
    }
}
