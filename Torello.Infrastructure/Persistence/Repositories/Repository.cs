using Torello.Application.Common.Interfaces.Persistence;
using Torello.Domain.Common.Primitives;

namespace Torello.Infrastructure.Persistence.Repositories;

public abstract class Repository<TEntity, TEntityId>: IRepository<TEntity, TEntityId>
    where TEntity : Entity<TEntityId>
{
    private readonly TorelloDbContext _dbContext;

    protected Repository(TorelloDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public virtual async Task<TEntity?> GetByIdAsync(TEntityId id)
    {
        return await _dbContext.Set<TEntity>().FindAsync(id);
    }

    public async Task AddAsync(TEntity entity)
    {
        await _dbContext.Set<TEntity>().AddAsync(entity);
    }

    public void Update(TEntity entity)
    {
        _dbContext.Set<TEntity>().Update(entity);
    }

    public void Remove(TEntity entity)
    {
        _dbContext.Set<TEntity>().Remove(entity);
    }
}