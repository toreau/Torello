using System.Net.Mime;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Torello.Application.Common;
using Torello.Application.Common.Interfaces.Persistence;
using Torello.Contracts;
using Torello.Domain.Common.Errors;
using Torello.Domain.Users;

namespace Torello.Application.Features.Projects.Queries;

public sealed record GetProjectsByUserIdQuery(
    Guid UserId
) : IRequest<ErrorOr<ProjectsResult>>;

[ApiExplorerSettings(GroupName = "Projects")]
public class GetProjectsByUserIdController : ApiController
{
    private readonly IMediator _mediator;

    public GetProjectsByUserIdController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("/users/{userId:guid}/projects", Name = nameof(ProjectsResult))]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ProjectsResponse), 200)]
    public async Task<IActionResult> GetProjectsByUserId(Guid userId)
    {
        var getProjectsByUserIdQuery = new GetProjectsByUserIdQuery(userId);
        var projectsResult = await _mediator.Send(getProjectsByUserIdQuery);

        return projectsResult.Match(
            result => Ok(result.ToResponse()),
            errors => Problem(errors)
        );
    }
}

internal sealed class GetProjectsByUserIdHandler : IRequestHandler<GetProjectsByUserIdQuery, ErrorOr<ProjectsResult>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProjectsByUserIdHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<ProjectsResult>> Handle(
        GetProjectsByUserIdQuery getProjectsByUserIdQuery,
        CancellationToken cancellationToken
    )
    {
        if (UserId.Create(getProjectsByUserIdQuery.UserId) is not { } userId)
            return Errors.EntityId.Invalid;

        if (await _unitOfWork.Users.GetByIdAsync(userId) is not { } user)
            return Errors.Users.NotFound;

        return new ProjectsResult(user.Projects);
    }
}
