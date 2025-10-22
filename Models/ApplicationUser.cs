using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
    
    // YENİ ALAN: Customer nesnesine bire-bir referans
    public Customer Customer { get; set; } = default!; 
}