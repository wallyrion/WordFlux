using Microsoft.AspNetCore.Identity;

namespace WordFlux.Domain;

public class AppUser : IdentityUser<Guid>
{
    public IEnumerable<ApplicationRole>? Roles { get; set; }
}


public class ApplicationRole : IdentityRole<Guid>
{
}

