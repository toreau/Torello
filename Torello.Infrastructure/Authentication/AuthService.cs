using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Torello.Application.Common.Interfaces;
using Torello.Application.Common.Interfaces.Persistence;
using Torello.Domain.Users;

namespace Torello.Infrastructure.Authentication;

public class AuthService(IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork) : IAuthService
{
    private UserId? GetCurrentUserId()
    {
        HttpContext? httpContext = httpContextAccessor.HttpContext;

        if (httpContext?.User.Identity?.IsAuthenticated != true)
            return null;

        var userIdStr = httpContext.User.Claims
            .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (userIdStr is null || UserId.Create(userIdStr) is not { } userId)
            return null;

        return userId;
    }

    public async Task<User?> GetCurrentUserAsync()
    {
        return GetCurrentUserId() is { } userId
            ? await unitOfWork.Users.GetByIdAsync(userId)
            : null;
    }
}
