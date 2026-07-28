using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReforaTec.Api.Entities;

namespace ReforaTec.Api.Database.Configurations;

public class CampaignConfiguration : IEntityTypeConfiguration<Campaign>
{
    public void Configure(EntityTypeBuilder<Campaign> builder)
    {
        builder.OwnsOne(c => c.Period);
        builder.OwnsOne(c => c.Location);

        builder.Property(c => c.CampaignName)
            .HasMaxLength(50);

        builder.Property(c => c.NormalizedCampaignName)
            .HasMaxLength(50);

        builder.Property(c => c.InscriptionCode)
            .HasMaxLength(10);

        // Multi-Tenant Composite Unique Index: Enforces campaign name uniqueness per institution,
        // while allowing different schools to use identical campaign names.
        builder.HasIndex(c => new { c.TenantId, c.NormalizedCampaignName })
            .IsUnique();

        builder.HasIndex(c => c.InscriptionCode)
            .IsUnique();

        builder.HasOne(c => c.Tenant)
            .WithMany()
            .HasForeignKey(c => c.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}