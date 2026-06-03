using System.Reflection;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Common.Interfaces;
using ReforaTec.Api.Entities;
using ReforaTec.Api.Entities.Common;

namespace ReforaTec.Api.Database;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Tree> Trees { get; set; }
    public DbSet<Value> Values { get; set; }
    public DbSet<Species> Species { get; set; }
    public DbSet<Campaign> Campaigns { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<AuditableEntity>();
        var now = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            switch (entry)
            {
                case { State: EntityState.Added }:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.ModifiedAt = now;
                    break;
                case { State: EntityState.Modified }:
                    entry.Entity.ModifiedAt = now;
                    break;
            }
        }

        var normalizableEntries = ChangeTracker.Entries<INormalizable>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified);

        foreach (var entry in normalizableEntries)
        {
            entry.Entity.Normalize();
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}