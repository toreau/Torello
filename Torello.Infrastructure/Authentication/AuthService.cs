using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Torello.Application.Common.Interfaces;
using Torello.Application.Common.Interfaces.Persistence;
using Torello.Domain.Users;

namespace Torello.Infrastructure.Authentication;

public class AuthService : IAuthService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUnitOfWork _unitOfWork;

    public AuthService(IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork)
    {
        _httpContextAccessor = httpContextAccessor;
        _unitOfWork = unitOfWork;
    }

    public UserId? GetLoggedInUserId()
    {
        HttpContext? httpContext = _httpContextAccessor.HttpContext;

        if (httpContext?.User?.Identity?.IsAuthenticated != true)
            return null;

        var userIdStr = httpContext.User.Claims
            .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (userIdStr is null || UserId.Create(userIdStr) is not { } userId)
            return null;

        return userId;
    }

    public async Task<User?> GetLoggedInUser()
    {
        return GetLoggedInUserId() is { } userId
            ? await _unitOfWork.Users.GetByIdAsync(userId)
            : null;
    }
}
