using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Torello.Application.Common;
using Torello.Application.Common.Interfaces.Persistence;
using Torello.Application.Features.Projects.Queries;
using Torello.Domain.Projects;

namespace Torello.Application.Features.Projects.Commands;

public sealed record CreateProjectRequest(
    string Title
)
{
    public CreateProjectCommand ToCommand()
        => new CreateProjectCommand(Title);
}


public sealed record CreateProjectCommand(
    string Name
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

internal sealed class CreateProjectValidator : AbstractValidator<CreateProjectCommand>
{
    internal CreateProjectValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("A project name must be specified!");
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
        var project = Project.Create(createProjectCommand.Name);

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
            Project.CreatedAt.ToString("s"));
}

internal sealed record CreateProjectResponse(
    string Id,
    string Title,
    string CreatedAt
);
