using Microsoft.AspNetCore.Identity;

public class AppUser : IdentityUser
{
    public IEnumerable<IdentityRole>? Roles { get; set; }
}