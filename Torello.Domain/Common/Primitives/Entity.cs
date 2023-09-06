namespace Torello.Domain.Common.Primitives;

public abstract class Entity<TEntityId>
{
    public TEntityId Id { get; protected set; }

    protected Entity(TEntityId id)
    {
        Id = id;
    }
}
