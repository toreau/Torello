using ErrorOr;

namespace Torello.Domain.Common.Errors;

public static partial class Errors
{
    public static class Users
    {
        public static Error NotFound => Error.NotFound(
            code: "User.NotFound",
            description: "User doesn't exist!"
        );

        public static Error UsernameAlreadyExists => Error.Conflict(
            code: "User.UsernameAlreadyExists",
            description: "Username already exists!"
        );

        public static Error InvalidCredentials => Error.Custom(
            type: CustomErrorTypes.Unauthorized,
            code: "User.InvalidCredentials",
            description: "Invalid credentials!"
        );
    }
}