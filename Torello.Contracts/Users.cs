using System.Data.SqlTypes;
using Torello.Domain.Users;

namespace Torello.Contracts;

public sealed record UserResult(User User)
{
    public UserResponse ToResponse() => new(User.Id.Value, User.Username, User.CreatedAt);
}

public sealed record UserResponse(SqlGuid Id, string Username, DateTimeOffset CreatedAt);
