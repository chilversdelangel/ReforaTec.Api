using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Entities;
using ReforaTec.Api.Entities.Common;

namespace ReforaTec.Api.Database;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Tree> Trees { get; set; }
    public DbSet<Value> Values { get; set; }
    public DbSet<Species> Species { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tree>().OwnsOne(t => t.Location);

        modelBuilder.Entity<Value>(builder =>
        {
            builder.Property(v => v.ValueName)
                .IsRequired()
                .HasMaxLength(25);

            builder.HasIndex(v => v.ValueName)
                .IsUnique();
        });

        modelBuilder.Entity<Species>(builder =>
        {
            builder.Property(s => s.ScientificName)
                .IsRequired()
                .HasMaxLength(100);
            
            builder.HasIndex(s => s.ScientificName)
                .IsUnique();
            
            builder.Property(s => s.CommonNames)
                .IsRequired()
                .HasColumnType("varchar(50)[]")
                .HasDefaultValueSql("'{}'::varchar(50)[]");
            
            builder.Property(s => s.Description)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(s => s.ImageUrl)
                .HasMaxLength(2048);
        });
    }

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

        return base.SaveChangesAsync(cancellationToken);
    }
}