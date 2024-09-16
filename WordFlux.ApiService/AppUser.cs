using Microsoft.AspNetCore.Identity;

namespace WordFlux.ApiService;

public class AppUser : IdentityUser
{
    public IEnumerable<IdentityRole>? Roles { get; set; }
}