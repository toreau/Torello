using Torello.Domain.Common.Primitives;

namespace Torello.Application.Common.Interfaces.Persistence;

public interface IRepository<TEntity, TEntityId> where TEntity : Entity<TEntityId>
{
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task<TEntity?> GetByIdAsync(TEntityId id);
    Task AddAsync(TEntity entity);
    void Remove(TEntity entity);
}