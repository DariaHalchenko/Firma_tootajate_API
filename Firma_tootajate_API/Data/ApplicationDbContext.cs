using Firma_tootajate_API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;

namespace Firma_tootajate_API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Login> Logins { get; set; }
        public DbSet<Tootajate> Tootajates { get; set; }
        public DbSet<Worktime> Worktimes { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
    }
}
