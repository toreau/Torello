using System.Net.Mime;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Torello.Application.Common;
using Torello.Application.Common.Interfaces;
using Torello.Application.Common.Interfaces.Persistence;
using Torello.Application.Features.Users.Queries;
using Torello.Contracts;
using Torello.Domain.Common.Errors;
using Torello.Domain.Users;

namespace Torello.Application.Features.Users.Commands;

public sealed record RegisterUserRequest(
    string Username,
    string Password
)
{
    public RegisterUserCommand ToCommand()
        => new RegisterUserCommand(
            Username,
            Password
        );
}

public sealed record RegisterUserCommand(
    string Username,
    string Password
) : IRequest<ErrorOr<UserResult>>;

[ApiExplorerSettings(GroupName = "Users")]
public sealed class RegisterUserController : ApiController
{
    private readonly IMediator _mediator;

    public RegisterUserController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("/users", Name = nameof(RegisterUser))]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(UserResponse), 201)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> RegisterUser(RegisterUserRequest registerUserRequest)
    {
        var registerUserCommand = registerUserRequest.ToCommand();
        var registerUserResult = await _mediator.Send(registerUserCommand);

        return registerUserResult.Match(
            result => CreatedAtRoute(
                nameof(GetUserByIdController.GetUserById),
                new { id = result.ToResponse().Id },
                result.ToResponse()),
            errors => Problem(errors)
        );
    }
}

public sealed class RegisterUserValidator : AbstractValidator<RegisterUserCommand>
{
    private const byte MinUsernameLength = 4;
    private const byte MaxUsernameLength = 24;
    private const byte MinPasswordLength = 8;
    private const byte MaxPasswordLength = byte.MaxValue;

    public RegisterUserValidator()
    {
        RuleFor(x => x.Username)
            .MinimumLength(MinUsernameLength).WithMessage($"The username must be minimum {MinUsernameLength} characters long!")
            .MaximumLength(MaxUsernameLength).WithMessage($"The username must be maximum {MaxUsernameLength} characters long!");

        RuleFor(x => x.Password)
            .MinimumLength(MinPasswordLength).WithMessage($"The password must be minimum {MinPasswordLength} characters long!")
            .MaximumLength(MaxPasswordLength).WithMessage($"The password must be maximum {MaxPasswordLength} characters long!");
    }
}

internal sealed class RegisterUserHandler : IRequestHandler<RegisterUserCommand, ErrorOr<UserResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterUserHandler(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task<ErrorOr<UserResult>> Handle(
        RegisterUserCommand registerUserCommand,
        CancellationToken cancellationToken
    )
    {
        // Username already exists?
        var user = await _unitOfWork.Users.GetByUsernameAsync(registerUserCommand.Username);
        if (user is not null)
            return Errors.Users.UsernameAlreadyExists;

        // Create a new user
        user = User.Create(
            registerUserCommand.Username,
            _passwordHasher.HashPassword(registerUserCommand.Password)
        );

        // Add and save
        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return new UserResult(user);
    }
}
