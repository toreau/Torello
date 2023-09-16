using Microsoft.EntityFrameworkCore;
using Torello.Application.Common.Interfaces.Persistence;
using Torello.Domain.Users;

namespace Torello.Infrastructure.Persistence.Repositories;

public class UserRepository : Repository<User, UserId>, IUserRepository
{
    public UserRepository(TorelloDbContext dbContext) : base(dbContext)
    {
    }

    public async override Task<User?> GetByIdAsync(UserId id)
    {
        return await _dbContext.Users
            .SingleOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _dbContext.Users
            .SingleOrDefaultAsync(u => u.Username.Equals(username));
    }
}