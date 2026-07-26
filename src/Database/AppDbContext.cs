using System.Reflection;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Common.Interfaces;
using ReforaTec.Api.Entities;
using ReforaTec.Api.Entities.Common;

namespace ReforaTec.Api.Database;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    #region Multi-Tenant & Identity
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<AuthOtpCode> AuthOtpCodes { get; set; }
    #endregion

    #region Master Catalogs
    public DbSet<Species> Species { get; set; }
    public DbSet<Value> Values { get; set; }
    public DbSet<ServiceType> ServiceTypes { get; set; }
    #endregion

    #region Core Domain Entities
    public DbSet<Campaign> Campaigns { get; set; }
    public DbSet<Tree> Trees { get; set; }
    #endregion

    #region Domain Junction Entities
    public DbSet<UserInspectsCampaign> UserInspectsCampaigns { get; set; }
    public DbSet<CampaignManagesTree> CampaignManagesTrees { get; set; }
    public DbSet<UserCaresForTree> UserCaresForTrees { get; set; }
    #endregion

    #region Field Activity Logs
    public DbSet<Service> Services { get; set; }
    public DbSet<Measurement> Measurements { get; set; }
    #endregion

    #region Push Notifications & Devices
    public DbSet<NotificationTemplate> NotificationTemplates { get; set; }
    public DbSet<UserDevice> UserDevices { get; set; }
    #endregion

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