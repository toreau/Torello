using Torello.Domain.Users;

namespace Torello.Application.Common.Interfaces;

public interface IAuthService
{
    Task<User?> GetLoggedInUser();
}
