using ErrorOr;

namespace Torello.Domain.Common.Errors;

public static partial class Errors
{
    public static class Lanes
    {
        public static Error NotFound => Error.NotFound(
            code: "Lane.NotFound",
            description: "Lane doesn't exist!"
        );
    }
}