using ErrorOr;

namespace Torello.Domain.Common.Errors;

public static partial class Errors
{
    public static class EntityId
    {
        public static Error Invalid => Error.Validation(
            code: "EntityId.Invalid",
            description: "Not a valid EntityId!"
        );
    }
}