using Microsoft.AspNetCore.Identity;

namespace DataAccess.Domain;

public class User : IdentityUser<int>
{
    public required string FirstName { get; set; }
}