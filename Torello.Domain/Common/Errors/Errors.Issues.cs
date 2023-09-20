using ErrorOr;

namespace Torello.Domain.Common.Errors;

public static partial class Errors
{
    public static class Issues
    {
        public static Error NotFound => Error.NotFound(
            code: "Issue.NotFound",
            description: "Issue doesn't exist!"
        );
    }
}
