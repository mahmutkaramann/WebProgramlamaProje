using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SporSalonu.Models;

namespace SporSalonu.Data
{
    public class AppDbContext: IdentityDbContext<Users>
    {
        public AppDbContext(DbContextOptions options) : base(options) 
        {
        }
        // Alternatif olarak:
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=UserApp;Trusted_Connection=True;");
            }
        }
    }
}
