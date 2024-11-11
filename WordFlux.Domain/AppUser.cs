using Microsoft.AspNetCore.Identity;

namespace WordFlux.Domain;

public class AppUser : IdentityUser
{
    public IEnumerable<IdentityRole>? Roles { get; set; }
}