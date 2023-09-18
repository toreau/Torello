using System.Net.Mime;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Torello.Application.Common;
using Torello.Application.Common.Interfaces;
using Torello.Application.Common.Interfaces.Persistence;
using Torello.Application.Features.Projects.Queries;
using Torello.Contracts;
using Torello.Domain.Common.Errors;
using Torello.Domain.Projects;

namespace Torello.Application.Features.Projects.Commands;

public sealed record CreateProjectRequest(
    string Title,
    string Description
)
{
    public UpsertProjectCommand ToCommand()
        => new UpsertProjectCommand(
            null,
            Title,
            Description
        );
}

public sealed record UpdateProjectRequest(
    Guid Id,
    string Title,
    string Description
)
{
    public UpsertProjectCommand ToCommand()
        => new UpsertProjectCommand(
            ProjectId.Create(Id),
            Title,
            Description
        );
}

public sealed record UpsertProjectCommand(
    ProjectId? Id,
    string Title,
    string Description
) : IRequest<ErrorOr<ProjectResult>>;

[ApiExplorerSettings(GroupName = "Projects")]
public sealed class UpsertProjectController : ApiController
{
    private readonly IMediator _mediator;

    public UpsertProjectController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("/projects", Name = nameof(CreateProject))]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ProjectResponse), 201)]
    public async Task<IActionResult> CreateProject(CreateProjectRequest createProjectRequest)
    {
        return await UpsertProject(createProjectRequest.ToCommand());
    }

    [HttpPut("/projects", Name = nameof(UpdateProject))]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ProjectResponse), 200)]
    public async Task<IActionResult> UpdateProject(UpdateProjectRequest updateProjectRequest)
    {
        return await UpsertProject(updateProjectRequest.ToCommand(), updateProjectRequest.Id);
    }

    private async Task<IActionResult> UpsertProject(UpsertProjectCommand upsertProjectCommand, Guid? id = null)
    {
        var upsertProjectResult = await _mediator.Send(upsertProjectCommand);

        if (id is null)
        {
            return upsertProjectResult.Match(
                result => CreatedAtRoute(
                    nameof(GetProjectByIdController.GetProjectById),
                    new { id = result.ToResponse().Id },
                    result.ToResponse()),
                errors => Problem(errors)
            );
        }
        else
        {
            return upsertProjectResult.Match(
                result => Ok(result.ToResponse()),
                errors => Problem(errors)
            );
        }
    }
}

public sealed class UpsertProjectValidator : AbstractValidator<UpsertProjectCommand>
{
    private const byte MinProjectNameLength = 4;
    private const byte MaxProjectNameLength = 64;

    public UpsertProjectValidator()
    {
         RuleFor(x => x.Title)
             .MinimumLength(MinProjectNameLength).WithMessage($"The project name must be minimum {MinProjectNameLength} characters long!")
             .MaximumLength(MaxProjectNameLength).WithMessage($"The project name must be maximum {MaxProjectNameLength} characters long!");
    }
}

internal sealed class UpsertProjectHandler : IRequestHandler<UpsertProjectCommand, ErrorOr<ProjectResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;

    public UpsertProjectHandler(IUnitOfWork unitOfWork, IAuthService authService)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
    }

    public async Task<ErrorOr<ProjectResult>> Handle(
        UpsertProjectCommand upsertProjectCommand,
        CancellationToken cancellationToken
    )
    {
        if (await _authService.GetCurrentUserAsync() is not { } user)
            return Errors.Users.InvalidCredentials;

        Project? project;

        // Update an existing project?
        if (upsertProjectCommand.Id is not null)
        {
            project = await _unitOfWork.Projects.GetByIdAsync(upsertProjectCommand.Id);
            if (project is null)
                return Errors.Projects.NotFound;

            if (project.User.Id != user.Id)
                return Errors.Users.InvalidCredentials;

            project.Update(
                upsertProjectCommand.Title,
                upsertProjectCommand.Description
            );
        }
        // Create a new project
        else
        {
            project = Project.Create(
                upsertProjectCommand.Title,
                upsertProjectCommand.Description
            );

            user.AddProject(project);
        }

        // Save and return the updated/created result
        await _unitOfWork.SaveChangesAsync();

        return new ProjectResult(project);
    }
}
