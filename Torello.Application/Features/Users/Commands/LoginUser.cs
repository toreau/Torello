using System.Net.Mime;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torello.Application.Common;
using Torello.Application.Common.Interfaces;
using Torello.Application.Common.Interfaces.Persistence;
using Torello.Domain.Common.Errors;
using Torello.Domain.Users;

namespace Torello.Application.Features.Users.Commands;

public sealed record LoginUserRequest(string Username, string Password)
{
    public LoginUserCommand ToCommand() => new(Username, Password);
}

public sealed record LoginUserCommand(string Username, string Password) : IRequest<ErrorOr<LoginUserResult>>;

[ApiExplorerSettings(GroupName = "Authentication")]
[AllowAnonymous]
public sealed class LoginUserController(ISender mediator) : ApiController
{
    [HttpPost("/login", Name = nameof(LoginUser))]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(LoginUserResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> LoginUser(LoginUserRequest loginUserRequest)
    {
        var loginUserCommand = loginUserRequest.ToCommand();
        var loginUserResult = await mediator.Send(loginUserCommand);

        return loginUserResult.Match(
            result => Ok(result.ToResponse()),
            errors => Problem(errors)
        );
    }
}

public sealed class LoginUserValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username must be specified!");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password must be specified!");
    }
}

internal sealed class LoginUserHandler(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    : IRequestHandler<LoginUserCommand, ErrorOr<LoginUserResult>>
{
    public async Task<ErrorOr<LoginUserResult>> Handle(LoginUserCommand loginUserCommand, CancellationToken cancellationToken)
    {
        // Username exists?
        if (await unitOfWork.Users.GetByUsernameAsync(loginUserCommand.Username) is not { } user)
            return Errors.Users.InvalidCredentials;

        // Password matches?
        if (!passwordHasher.VerifyPassword(loginUserCommand.Password, user.HashedPassword))
            return Errors.Users.InvalidCredentials;

        // Great success!
        return new LoginUserResult(
            user,
            jwtTokenGenerator.GenerateToken(user)
        );
    }
}

internal sealed record LoginUserResult(User User, string JwtToken)
{
    public LoginUserResponse ToResponse() => new(User.Id.Value, User.Username, JwtToken);
}

internal sealed record LoginUserResponse(Guid Id, string Username, string JwtToken);
