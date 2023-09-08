using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Torello.Application.Common;
using Torello.Application.Common.Interfaces.Persistence;
using Torello.Application.Features.Projects.Queries;
using Torello.Domain.Boards;
using Torello.Domain.Lanes;
using Torello.Domain.Projects;

namespace Torello.Application.Features.Projects.Commands;

public sealed record CreateProjectRequest(
    string Title,
    string Description
)
{
    public CreateProjectCommand ToCommand()
        => new CreateProjectCommand(Title, Description);
}


public sealed record CreateProjectCommand(
    string Title,
    string Description
) : IRequest<ErrorOr<CreateProjectResult>>;

[ApiExplorerSettings(GroupName = "Projects")]
public sealed class CreateProjectController : ApiController
{
    private readonly IMediator _mediator;

    public CreateProjectController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("/projects", Name = "CreateProject")]
    public async Task<IActionResult> CreateProject(CreateProjectRequest createProjectRequest)
    {
        var createProjectCommand = createProjectRequest.ToCommand();
        var createProjectResult = await _mediator.Send(createProjectCommand);

        return createProjectResult.Match(
            result => CreatedAtRoute(nameof(GetProjectByIdController.GetProjectById), new { id = result.ToResponse().Id }, result.ToResponse()),
            errors => Problem(errors)
        );
    }
}

public sealed class CreateProjectValidator : AbstractValidator<CreateProjectCommand>
{
    private const byte MinProjectNameLength = 4;
    private const byte MaxProjectNameLength = 64;

    public CreateProjectValidator()
    {
        RuleFor(x => x.Title)
            .MinimumLength(MinProjectNameLength).WithMessage($"The project name must be minimum {MinProjectNameLength} characters long!")
            .MaximumLength(MaxProjectNameLength).WithMessage($"The project name must be maximum {MaxProjectNameLength} characters long!");
    }
}

internal sealed class CreateProjectHandler : IRequestHandler<CreateProjectCommand, ErrorOr<CreateProjectResult>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateProjectHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<CreateProjectResult>> Handle(
        CreateProjectCommand createProjectCommand,
        CancellationToken cancellationToken
    )
    {
        // Create a new project with some defaults
        var project = Project.Create(
            createProjectCommand.Title,
            createProjectCommand.Description
        );

        var board = Board.Create("Default board");
        foreach (var laneTitle in new[] { "Backlog", "Todo", "Doing", "Done" })
        {
            var lane = Lane.Create(laneTitle);
            board.AddLane(lane);
        }

        project.AddBoard(board);

        await _unitOfWork.Projects.AddAsync(project);
        await _unitOfWork.SaveChangesAsync();

        return new CreateProjectResult(project);
    }
}

internal sealed record CreateProjectResult(
    Project Project
)
{
    public CreateProjectResponse ToResponse()
        => new CreateProjectResponse(
            Project.Id.Value.ToString(),
            Project.Title,
            Project.Description,
            Project.CreatedAt.ToString("s"));
}

internal sealed record CreateProjectResponse(
    string Id,
    string Title,
    string Description,
    string CreatedAt
);
