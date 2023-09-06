using ErrorOr;

namespace Torello.Domain.Common.Errors;

public static partial class Errors
{
    public static class Guid
    {
        public static Error Invalid => Error.Validation(
            code: "Guid.Invalid",
            description: "Not a valid GUID!"
        );
    }
}