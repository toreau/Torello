using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Torello.Application.Common;
using Torello.Application.Common.Interfaces.Persistence;
using Torello.Application.Features.Projects.Queries;
using Torello.Domain.Boards;
using Torello.Domain.Common.Errors;
using Torello.Domain.Lanes;
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
            ProjectId.Create(Id.ToString()),
            Title,
            Description
        );
}

public sealed record UpsertProjectCommand(
    ProjectId? Id,
    string Title,
    string Description
) : IRequest<ErrorOr<UpsertProjectResult>>;

[ApiExplorerSettings(GroupName = "Projects")]
public sealed class UpsertProjectController : ApiController
{
    private readonly IMediator _mediator;

    public UpsertProjectController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("/projects", Name = "CreateProject")]
    public async Task<IActionResult> CreateProject(CreateProjectRequest createProjectRequest)
    {
        return await UpsertProject(createProjectRequest.ToCommand());
    }

    [HttpPut("/projects", Name = "UpdateProject")]
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

internal sealed class UpsertProjectHandler : IRequestHandler<UpsertProjectCommand, ErrorOr<UpsertProjectResult>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpsertProjectHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<UpsertProjectResult>> Handle(
        UpsertProjectCommand upsertProjectCommand,
        CancellationToken cancellationToken
    )
    {
        Project? project;

        // Update an existing project?
        if (upsertProjectCommand.Id is not null)
        {
            project = await _unitOfWork.Projects.GetByIdAsync(upsertProjectCommand.Id);
            if (project is null)
                return Errors.Projects.NotFound;

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

            var board = Board.Create("Default board");

            foreach (var laneTitle in new[] { "Backlog", "Todo", "Doing", "Done" })
                board.AddLane(Lane.Create(laneTitle));

            project.AddBoard(board);

            await _unitOfWork.Projects.AddAsync(project);
        }

        // Save and return upserted result
        await _unitOfWork.SaveChangesAsync();

        return new UpsertProjectResult(project);

    }
}

internal sealed record UpsertProjectResult(
    Project Project
)
{
    public UpsertProjectResponse ToResponse()
        => new UpsertProjectResponse(
            Project.Id.Value,
            Project.Title,
            Project.Description
        );
}

internal sealed record UpsertProjectResponse(
    Guid Id,
    string Title,
    string Description
);
