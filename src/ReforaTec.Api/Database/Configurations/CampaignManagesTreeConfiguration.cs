using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReforaTec.Api.Entities;

namespace ReforaTec.Api.Database.Configurations;

public class CampaignManagesTreeConfiguration : IEntityTypeConfiguration<CampaignManagesTree>
{
    public void Configure(EntityTypeBuilder<CampaignManagesTree> builder)
    {
        builder.Property(c => c.CampaignFolio)
            .HasMaxLength(10);

        builder.Property(c => c.NormalizedCampaignFolio)
            .HasMaxLength(10);

        builder.HasIndex(c => c.TenantId);
        builder.HasIndex(c => c.CampaignId);
        builder.HasIndex(c => c.TreeId);

        // Unique Composite Index: Enforces human-readable folio uniqueness within a specific campaign
        // (e.g. folio "0034" cannot be assigned to two different trees in the same campaign).
        builder.HasIndex(c => new { c.CampaignId, c.NormalizedCampaignFolio })
            .IsUnique();

        builder.HasOne(c => c.Tenant)
            .WithMany()
            .HasForeignKey(c => c.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Campaign)
            .WithMany()
            .HasForeignKey(c => c.CampaignId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Tree)
            .WithMany()
            .HasForeignKey(c => c.TreeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
