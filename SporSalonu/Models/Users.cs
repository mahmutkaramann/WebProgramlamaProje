using Microsoft.AspNetCore.Identity;

namespace SporSalonu.Models
{
    // Login için;
    public class Users: IdentityUser
    {  
        public  string FullName { get; set; }
    }
}
