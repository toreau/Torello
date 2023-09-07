using Torello.Domain.Common.Primitives;

namespace Torello.Application.Common.Interfaces.Persistence;

public interface IRepository<TEntity, TEntityId> where TEntity : Entity<TEntityId>
{
    Task<IEnumerable<TEntity>> GetAll();
    Task<TEntity?> GetByIdAsync(TEntityId id);
    Task AddAsync(TEntity entity);
    void Update(TEntity entity);
    void Remove(TEntity entity);
}