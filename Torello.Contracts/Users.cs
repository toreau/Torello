using Torello.Domain.Users;

namespace Torello.Contracts;

public sealed record UserResult(
    User User
)
{
    public UserResponse ToResponse()
        => new UserResponse(
            User.Id.Value,
            User.Username,
            User.CreatedAt
        );
}

public sealed record UserResponse(
    Guid Id,
    string Username,
    DateTimeOffset CreatedAt
);
