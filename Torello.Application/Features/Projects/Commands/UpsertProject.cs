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

public sealed record UpsertProjectRequest(
    string Title,
    string Description
)
{
    public UpsertProjectCommand ToCommand(ProjectId? id = null)
        => new UpsertProjectCommand(
            Title,
            Description,
            id
        );
}

public sealed record UpsertProjectCommand(
    string Title,
    string Description,
    ProjectId? ProjectId
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
    public async Task<IActionResult> CreateProject(UpsertProjectRequest upsertProjectRequest)
    {
        return await UpsertProject(upsertProjectRequest.ToCommand());
    }

    [HttpPut("/projects/{projectId:guid}", Name = nameof(UpdateProject))]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ProjectResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateProject(Guid projectId, UpsertProjectRequest upsertProjectRequest)
    {
        return await UpsertProject(upsertProjectRequest.ToCommand(ProjectId.Create(projectId)));
    }

    private async Task<IActionResult> UpsertProject(UpsertProjectCommand upsertProjectCommand)
    {
        var upsertProjectResult = await _mediator.Send(upsertProjectCommand);

        return upsertProjectResult.Match(
            result =>
            {
                var response = result.ToResponse();

                return upsertProjectCommand.ProjectId is null
                    ? CreatedAtRoute(
                        nameof(GetProjectByIdController.GetProjectById),
                        new { projectId = response.Id },
                        response)
                    : Ok(response);
            },
            errors => Problem(errors)
        );
    }
}

public sealed class UpsertProjectValidator : AbstractValidator<UpsertProjectCommand>
{
    private const byte MinProjectTitleLength = 4;
    private const byte MaxProjectTitleLength = 64;

    public UpsertProjectValidator()
    {
         RuleFor(x => x.Title)
             .MinimumLength(MinProjectTitleLength).WithMessage($"The project title must be minimum {MinProjectTitleLength} characters long!")
             .MaximumLength(MaxProjectTitleLength).WithMessage($"The project title must be maximum {MaxProjectTitleLength} characters long!");
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
        if (upsertProjectCommand.ProjectId is not null)
        {
            // Does the project exist?
            project = await _unitOfWork.Projects.GetByIdAsync(upsertProjectCommand.ProjectId);
            if (project is null)
                return Errors.Projects.NotFound;

            // Is the project's owner the same as the one logged in?
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
