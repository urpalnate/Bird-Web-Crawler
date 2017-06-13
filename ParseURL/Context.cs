using System.Data.Entity;

namespace ParseURL
{
    public class Context : DbContext
    {
        public DbSet<Bird> Birds { get; set; }
    }
}
