using ErrorOr;

namespace Torello.Domain.Common.Errors;

public static partial class Errors
{
    public static class Boards
    {
        public static Error NotFound => Error.NotFound(
            code: "Board.NotFound",
            description: "Board doesn't exist!"
        );
    }
}