using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Torello.Application.Common;
using Torello.Application.Common.Interfaces;
using Torello.Application.Common.Interfaces.Persistence;
using Torello.Domain.Common.Errors;
using Torello.Domain.Users;

namespace Torello.Application.Features.Users.Commands;

public sealed record LoginUserRequest(
    string Username,
    string Password
)
{
    public LoginUserCommand ToCommand()
        => new LoginUserCommand(
            Username,
            Password
        );
}

public sealed record LoginUserCommand(
    string Username,
    string Password
) : IRequest<ErrorOr<LoginUserResult>>;

[ApiExplorerSettings(GroupName = "Authentication")]
public sealed class LoginUserController : ApiController
{
    private readonly IMediator _mediator;

    public LoginUserController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("/login", Name = nameof(LoginUser))]
    public async Task<IActionResult> LoginUser(LoginUserRequest loginUserRequest)
    {
        var loginUserCommand = loginUserRequest.ToCommand();
        var loginUserResult = await _mediator.Send(loginUserCommand);

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

internal sealed class LoginUserHandler : IRequestHandler<LoginUserCommand, ErrorOr<LoginUserResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginUserHandler(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator
    )
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<ErrorOr<LoginUserResult>> Handle(
        LoginUserCommand loginUserCommand,
        CancellationToken cancellationToken
    )
    {
        // Username exists?
        if (await _unitOfWork.Users.GetByUsernameAsync(loginUserCommand.Username) is not { } user)
            return Errors.Users.InvalidCredentials;

        // Password matches?
        if (!_passwordHasher.VerifyPassword(loginUserCommand.Password, user.HashedPassword))
            return Errors.Users.InvalidCredentials;

        // Great success!
        return new LoginUserResult(
            user,
            _jwtTokenGenerator.GenerateToken(user)
        );
    }
}

internal sealed record LoginUserResult(
    User User,
    string JwtToken
)
{
    public LoginUserResponse ToResponse()
        => new LoginUserResponse(
            User.Id.Value,
            User.Username,
            JwtToken
        );
}

internal sealed record LoginUserResponse(
    Guid Id,
    string Username,
    string JwtToken
);
