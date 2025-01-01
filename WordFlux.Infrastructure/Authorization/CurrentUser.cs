using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using WordFlux.Application;
using WordFlux.Domain;

namespace WordFlux.Infrastructure.Authorization;


public class CurrentUser(IHttpContextAccessor httpContextAccessor, UserManager<AppUser> userManager) : ICurrentUser
{
    public string GetUserId()
    {
        var claimsPrincipal = httpContextAccessor.HttpContext?.User 
                              ?? throw new InvalidOperationException("No user context available.");

        var userId = userManager.GetUserId(claimsPrincipal);

        return userId!;
    }
}