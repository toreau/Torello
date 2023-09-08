using Torello.Domain.Users;

namespace Torello.Application.Common.Interfaces.Persistence;

public interface IUserRepository : IRepository<User, UserId>
{
}