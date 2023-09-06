using ErrorOr;

namespace Torello.Domain.Common.Errors;

public static partial class Errors
{
    public static class Projects
    {
        public static Error NotFound => Error.NotFound(
            code: "Project.NotFound",
            description: "Project doesn't exist!"
        );
    }
}