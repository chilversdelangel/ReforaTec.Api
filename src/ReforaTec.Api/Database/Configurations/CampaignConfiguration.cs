using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReforaTec.Api.Entities;

namespace ReforaTec.Api.Database.Configurations;

internal sealed class CampaignConfiguration : IEntityTypeConfiguration<Campaign>
{
    public void Configure(EntityTypeBuilder<Campaign> builder)
    {
        builder.OwnsOne(campaign => campaign.Period);

        builder.Property(campaign => campaign.Code)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(campaign => campaign.NormalizedCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(campaign => campaign.TenantId);

        builder.HasIndex(campaign => new { campaign.TenantId, campaign.NormalizedCode })
            .IsUnique();

        builder.HasOne(campaign => campaign.Tenant)
            .WithMany()
            .HasForeignKey(campaign => campaign.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
