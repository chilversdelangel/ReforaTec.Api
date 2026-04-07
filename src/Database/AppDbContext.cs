using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Entities;

namespace ReforaTec.Api.Database;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Tree> Trees { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tree>().OwnsOne(t => t.Location);
    }
}