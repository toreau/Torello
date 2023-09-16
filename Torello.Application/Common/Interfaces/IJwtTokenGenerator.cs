using Torello.Domain.Users;

namespace Torello.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user, string? jti = null);
}
