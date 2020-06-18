using Microsoft.EntityFrameworkCore;

namespace cinemapandas4.Models
{
    public class MyContext : DbContext
    {
        public MyContext (DbContextOptions options) : base (options) { }

        public DbSet<User> Users { get; set; }

        public DbSet<Movie> Movies { get; set; }
        public DbSet<WatchParty> Parties { get; set; }
    }
}