namespace Torello.Application.Common.Interfaces.Persistence;

public interface IUnitOfWork : IDisposable
{
    IProjectRepository Projects { get; }
    IBoardRepository Boards { get; }
    ILaneRepository Lanes { get; }
    IIssueRepository Issues { get; }

    Task<int> SaveChangesAsync();
}