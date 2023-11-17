using System.Net.Mime;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Torello.Application.Common;
using Torello.Application.Common.Interfaces;
using Torello.Application.Common.Interfaces.Persistence;
using Torello.Contracts;
using Torello.Domain.Common.Errors;
using Torello.Domain.Projects;

namespace Torello.Application.Features.Boards.Queries;

internal sealed record GetAllBoardsQuery(Guid ProjectId) : IRequest<ErrorOr<BoardsResult>>;

[ApiExplorerSettings(GroupName = "Boards")]
public sealed class GetAllBoardsController(ISender mediator) : ApiController
{
    [HttpGet("/projects/{projectId:guid}/boards")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(BoardsResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetAllBoards(Guid projectId)
    {
        var getAllBoardsQuery = new GetAllBoardsQuery(projectId);
        var boardsResult = await mediator.Send(getAllBoardsQuery);

        return boardsResult.Match(
            result => Ok(result.ToResponse()),
            Problem
        );
    }
}

internal sealed class GetAllBoardsHandler(IUnitOfWork unitOfWork, IAuthService authService) : IRequestHandler<GetAllBoardsQuery, ErrorOr<BoardsResult>>
{
    public async Task<ErrorOr<BoardsResult>> Handle(GetAllBoardsQuery getAllBoardsQuery, CancellationToken cancellationToken)
    {
        if (ProjectId.Create(getAllBoardsQuery.ProjectId) is not { } projectId)
            return Errors.EntityId.Invalid;

        if (await authService.GetCurrentUserAsync() is not { } user)
            return Errors.Users.InvalidCredentials;

        if (await unitOfWork.Projects.GetByIdAsync(projectId) is not { } project)
            return Errors.Projects.NotFound;

        if (project.User.Id != user.Id)
            return Errors.Users.InvalidCredentials;

        return new BoardsResult(project.Boards);
    }
}
