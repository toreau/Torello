using System.Net.Mime;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Torello.Application.Common;
using Torello.Application.Common.Interfaces.Persistence;
using Torello.Contracts;
using Torello.Domain.Common.Errors;
using Torello.Domain.Users;

namespace Torello.Application.Features.Users.Queries;

public sealed record GetUserByIdQuery(
    Guid Id
) : IRequest<ErrorOr<UserResult>>;

[ApiExplorerSettings(GroupName = "Users")]
public sealed class GetUserByIdController : ApiController
{
    private readonly IMediator _mediator;

    public GetUserByIdController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("/users/{id:guid}", Name = nameof(GetUserById))]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(UserResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var getUserByIdQuery = new GetUserByIdQuery(id);
        var getUserByIdResult = await _mediator.Send(getUserByIdQuery);

        return getUserByIdResult.Match(
            result => Ok(result.ToResponse()),
            errors => Problem(errors)
        );
    }
}

internal sealed class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, ErrorOr<UserResult>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUserByIdHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<UserResult>> Handle(
        GetUserByIdQuery getUserByIdQuery,
        CancellationToken cancellationToken
    )
    {
        if (UserId.Create(getUserByIdQuery.Id) is not { } userId)
            return Errors.EntityId.Invalid;

        if (await _unitOfWork.Users.GetByIdAsync(userId) is not { } user)
            return Errors.Users.NotFound;

        return new UserResult(user);
    }
}
