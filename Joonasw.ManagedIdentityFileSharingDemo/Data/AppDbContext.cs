using Microsoft.EntityFrameworkCore;

namespace Joonasw.ManagedIdentityFileSharingDemo.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<StoredFile> StoredFiles { get; set; }
    }
}
