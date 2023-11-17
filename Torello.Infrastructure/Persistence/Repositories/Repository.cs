using Microsoft.EntityFrameworkCore;
using Torello.Application.Common.Interfaces.Persistence;
using Torello.Domain.Common.Primitives;

namespace Torello.Infrastructure.Persistence.Repositories;

public abstract class Repository<TEntity, TEntityId>(TorelloDbContext dbContext) : IRepository<TEntity, TEntityId>
    where TEntity : Entity<TEntityId>
{
    protected readonly TorelloDbContext _dbContext = dbContext;

    public virtual IEnumerable<TEntity> GetAll()
    {
        return _dbContext.Set<TEntity>().ToList();
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        return await _dbContext.Set<TEntity>().ToListAsync();
    }

    public virtual TEntity? GetById(TEntityId id)
    {
        return _dbContext.Set<TEntity>()
            .SingleOrDefault(e => e.Id!.Equals(id));
    }

    public virtual async Task<TEntity?> GetByIdAsync(TEntityId id)
    {
        return await _dbContext.Set<TEntity>()
            .SingleOrDefaultAsync(e => e.Id!.Equals(id));
    }

    public void Add(TEntity entity)
    {
        _dbContext.Set<TEntity>().Add(entity);
    }

    public async Task AddAsync(TEntity entity)
    {
        await _dbContext.Set<TEntity>().AddAsync(entity);
    }

    public void Remove(TEntity entity)
    {
        _dbContext.Set<TEntity>().Remove(entity);
    }
}
