using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReforaTec.Api.Entities;

namespace ReforaTec.Api.Database.Configurations;

public class CampaignsConfiguration : IEntityTypeConfiguration<Campaign>
{
    public void Configure(EntityTypeBuilder<Campaign> builder)
    {
        builder.OwnsOne(c => c.Period);
        builder.OwnsOne(c => c.Location);


        builder.Property(c => c.CampaignName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.NormalizedCampaignName)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(c => c.NormalizedCampaignName)
            .IsUnique();
    }
}