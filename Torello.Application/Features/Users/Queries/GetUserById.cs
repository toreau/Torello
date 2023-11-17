using System.Net.Mime;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torello.Application.Common;
using Torello.Application.Common.Interfaces.Persistence;
using Torello.Contracts;
using Torello.Domain.Common.Errors;
using Torello.Domain.Users;

namespace Torello.Application.Features.Users.Queries;

public sealed record GetUserByIdQuery(Guid Id) : IRequest<ErrorOr<UserResult>>;

[ApiExplorerSettings(GroupName = "Users")]
[AllowAnonymous]
public sealed class GetUserByIdController(ISender mediator) : ApiController
{
    [HttpGet("/users/{userId:guid}", Name = nameof(GetUserById))]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(UserResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetUserById(Guid userId)
    {
        var getUserByIdQuery = new GetUserByIdQuery(userId);
        var getUserByIdResult = await mediator.Send(getUserByIdQuery);

        return getUserByIdResult.Match(
            result => Ok(result.ToResponse()),
            Problem
        );
    }
}

internal sealed class GetUserByIdHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetUserByIdQuery, ErrorOr<UserResult>>
{
    public async Task<ErrorOr<UserResult>> Handle(GetUserByIdQuery getUserByIdQuery, CancellationToken cancellationToken)
    {
        if (UserId.Create(getUserByIdQuery.Id) is not { } userId)
            return Errors.EntityId.Invalid;

        if (await unitOfWork.Users.GetByIdAsync(userId) is not { } user)
            return Errors.Users.NotFound;

        return new UserResult(user);
    }
}
