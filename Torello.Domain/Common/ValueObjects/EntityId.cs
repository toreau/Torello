using ValueOf;

namespace Torello.Domain.Common.ValueObjects;

public class EntityId<TThis> : ValueOf<Guid, TThis> where TThis : EntityId<TThis>, new()
{
    public static TThis CreateUnique() => From(Guid.NewGuid());
    public static TThis? Create(string value)
    {
        return Guid.TryParse(value, out var guid)
            ? From(guid)
            : null;
    }
}
