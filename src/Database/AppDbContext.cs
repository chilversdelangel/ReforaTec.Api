using System.Reflection;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Common.Interfaces;
using ReforaTec.Api.Entities;
using ReforaTec.Api.Entities.Common;

namespace ReforaTec.Api.Database;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    /// <summary>
    /// Multi-tenant educational institutions / schools.
    /// </summary>
    public DbSet<Tenant> Tenants { get; set; }

    /// <summary>
    /// Platform users (students, inspectors, coordinators, admins) and OTP auth codes.
    /// </summary>
    public DbSet<User> Users { get; set; }
    public DbSet<AuthOtpCode> AuthOtpCodes { get; set; }

    /// <summary>
    /// System master catalogs (botanical species, ecosystem values, service types).
    /// </summary>
    public DbSet<Species> Species { get; set; }
    public DbSet<Value> Values { get; set; }
    public DbSet<ServiceType> ServiceTypes { get; set; }

    /// <summary>
    /// Core domain entities (environmental campaigns and individual trees).
    /// </summary>
    public DbSet<Campaign> Campaigns { get; set; }
    public DbSet<Tree> Trees { get; set; }

    /// <summary>
    /// Domain junction entities (inspector assignments, campaign tree folios, student care relationships).
    /// </summary>
    public DbSet<UserInspectsCampaign> UserInspectsCampaigns { get; set; }
    public DbSet<CampaignManagesTree> CampaignManagesTrees { get; set; }
    public DbSet<UserCaresForTree> UserCaresForTrees { get; set; }

    /// <summary>
    /// Field activity logs (tree care services and botanical inspections).
    /// </summary>
    public DbSet<Service> Services { get; set; }
    public DbSet<Measurement> Measurements { get; set; }

    /// <summary>
    /// Push notification templates and user FCM mobile devices.
    /// </summary>
    public DbSet<NotificationTemplate> NotificationTemplates { get; set; }
    public DbSet<UserDevice> UserDevices { get; set; }

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