using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Torello.Application.Common;
using Torello.Application.Common.Interfaces.Persistence;
using Torello.Application.Features.Projects.Queries;
using Torello.Domain.Common.Errors;
using Torello.Domain.Projects;

namespace Torello.Application.Features.Projects.Commands;

public sealed record UpdateProjectRequest(
    string Id,
    string Title,
    string Description
)
{
    public UpdateProjectCommand ToCommand()
        => new UpdateProjectCommand(
            ProjectId.Create(Id),
            Title,
            Description
        );
}

public sealed record UpdateProjectCommand(
    ProjectId? Id,
    string Title,
    string Description
) : IRequest<ErrorOr<UpdateProjectResult>>;

[ApiExplorerSettings(GroupName = "Projects")]
public sealed class UpsertProjectController : ApiController
{
    private readonly IMediator _mediator;

    public UpsertProjectController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPut("/projects", Name = "UpdateProject")]
    public async Task<IActionResult> UpdateProject(UpdateProjectRequest updateProjectRequest)
    {
        var updateProjectCommand = updateProjectRequest.ToCommand();
        var updateProjectResult = await _mediator.Send(updateProjectCommand);

        return updateProjectResult.Match(
            result => CreatedAtRoute(
                nameof(GetProjectByIdController.GetProjectById),
                new { id = result.ToResponse().Id },
                result.ToResponse()),
            errors => Problem(errors)
        );
    }
}

// TODO: 'Create' and 'Update' should share the validator
public sealed class UpsertProjectValidator : AbstractValidator<UpdateProjectCommand>
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

internal sealed class UpsertProjectHandler : IRequestHandler<UpdateProjectCommand, ErrorOr<UpdateProjectResult>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpsertProjectHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<UpdateProjectResult>> Handle(
        UpdateProjectCommand updateProjectCommand,
        CancellationToken cancellationToken
    )
    {
        // TODO: Consider moving this to the validator
        if (updateProjectCommand.Id is null)
            return Errors.EntityId.Invalid;

        if (await _unitOfWork.Projects.GetByIdAsync(updateProjectCommand.Id) is not { } project)
            return Errors.Projects.NotFound;

        project.Update(
            updateProjectCommand.Title,
            updateProjectCommand.Description
        );

        _unitOfWork.Projects.Update(project);
        await _unitOfWork.SaveChangesAsync();

        return new UpdateProjectResult(project);
    }
}

internal sealed record UpdateProjectResult(
    Project Project
)
{
    public UpdateProjectResponse ToResponse()
        => new UpdateProjectResponse(
            Project.Id.Value.ToString(),
            Project.Title,
            Project.Description,
            Project.CreatedAt.ToString("s")
        );
}

internal sealed record UpdateProjectResponse(
    string Id,
    string Title,
    string Description,
    string CreatedAt
);