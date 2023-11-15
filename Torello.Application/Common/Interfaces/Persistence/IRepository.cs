using Torello.Domain.Common.Primitives;

namespace Torello.Application.Common.Interfaces.Persistence;

public interface IRepository<TEntity, TEntityId> where TEntity : Entity<TEntityId>
{
    IEnumerable<TEntity> GetAll();
    Task<IEnumerable<TEntity>> GetAllAsync();
    TEntity? GetById(TEntityId id);
    Task<TEntity?> GetByIdAsync(TEntityId id);
    void Add(TEntity entity);
    Task AddAsync(TEntity entity);
    void Remove(TEntity entity);
}