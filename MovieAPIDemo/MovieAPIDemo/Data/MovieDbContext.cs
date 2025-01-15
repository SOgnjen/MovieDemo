using Microsoft.EntityFrameworkCore;
using MovieAPIDemo.Entities;

namespace MovieAPIDemo.Data
{
    public class MovieDbContext : DbContext
    {
        public MovieDbContext(DbContextOptions<MovieDbContext> options) : base(options)
        {
            
        }

        public DbSet<Movie> Movie { get; set; }
        public DbSet<Person> Person { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
