using System.Data.Entity;

namespace ParseURL
{
    public class Context : DbContext
    {
        public DbSet<Bird> Birds { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Family> Families { get; set; }
    }
}
