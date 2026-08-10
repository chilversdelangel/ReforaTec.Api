using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReforaTec.Api.Entities;

namespace ReforaTec.Api.Database.Configurations;

public class UserInspectsCampaignConfiguration : IEntityTypeConfiguration<UserInspectsCampaign>
{
    public void Configure(EntityTypeBuilder<UserInspectsCampaign> builder)
    {
        builder.HasIndex(u => u.TenantId);
        builder.HasIndex(u => u.UserId);
        builder.HasIndex(u => u.CampaignId);

        builder.HasOne(u => u.Tenant)
            .WithMany()
            .HasForeignKey(u => u.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.User)
            .WithMany()
            .HasForeignKey(u => u.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.Campaign)
            .WithMany()
            .HasForeignKey(u => u.CampaignId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
