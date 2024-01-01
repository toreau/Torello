using SequentialGuid;
using ValueOf;

namespace Torello.Domain.Common.Primitives;

public class EntityId<TThis> : ValueOf<Guid, TThis> where TThis : EntityId<TThis>, new()
{
    public static TThis CreateUnique() => From(SequentialGuidGenerator.Instance.NewGuid());

    public static TThis? Create(string value)
    {
        return Guid.TryParse(value, out var guid)
            ? From(guid)
            : null;
    }

    public static TThis? Create(Guid value)
    {
        return From(value);
    }
}
