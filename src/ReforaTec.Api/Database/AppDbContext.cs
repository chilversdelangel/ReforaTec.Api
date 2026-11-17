using System.Reflection;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Common.Interfaces;
using ReforaTec.Api.Entities;
using ReforaTec.Api.Entities.Common;

namespace ReforaTec.Api.Database;

public sealed class AppDbContext(DbContextOptions options) : DbContext(options)
{
    #region Multi-Tenant & Identity
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<AuthOtpCode> AuthOtpCodes { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    #endregion

    #region Master Catalogs
    public DbSet<Species> Species { get; set; }
    public DbSet<Virtue> Virtues { get; set; }
    public DbSet<CareType> CareTypes { get; set; }
    #endregion

    #region Core Domain Entities
    public DbSet<Campaign> Campaigns { get; set; }
    public DbSet<Tree> Trees { get; set; }
    #endregion

    #region Domain Junction Entities
    public DbSet<CampaignEnrollment> CampaignEnrollments { get; set; }
    public DbSet<CareAssignment> CareAssignments { get; set; }
    #endregion

    #region Field Activity Logs
    public DbSet<CareLog> CareLogs { get; set; }
    public DbSet<Measurement> Measurements { get; set; }
    public DbSet<Inspection> Inspections { get; set; }
    #endregion

    #region Push Notifications & Devices
    public DbSet<NotificationTemplate> NotificationTemplates { get; set; }
    public DbSet<UserDevice> UserDevices { get; set; }
    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
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

        foreach (var entry in ChangeTracker.Entries<CreatableEntity>())
        {
            switch (entry)
            {
                case { State: EntityState.Added }:
                    entry.Entity.CreatedAt = now;
                    break;
            }
        }

        var normalizableEntries = ChangeTracker.Entries<INormalizable>()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified);

        foreach (var entry in normalizableEntries)
        {
            entry.Entity.Normalize();
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}