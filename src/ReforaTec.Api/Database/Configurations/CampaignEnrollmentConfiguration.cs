using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReforaTec.Api.Entities;

namespace ReforaTec.Api.Database.Configurations;

internal sealed class CampaignEnrollmentConfiguration : IEntityTypeConfiguration<CampaignEnrollment>
{
    public void Configure(EntityTypeBuilder<CampaignEnrollment> builder)
    {
        builder.Property(enrollment => enrollment.EvaluationNotes)
            .HasMaxLength(500);

        builder.HasIndex(enrollment => enrollment.TenantId);
        builder.HasIndex(enrollment => enrollment.UserId);

        builder.HasIndex(enrollment => new { enrollment.CampaignId, enrollment.UserId })
            .IsUnique();

        builder.HasOne(enrollment => enrollment.Tenant)
            .WithMany()
            .HasForeignKey(enrollment => enrollment.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(enrollment => enrollment.Campaign)
            .WithMany()
            .HasForeignKey(enrollment => enrollment.CampaignId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(enrollment => enrollment.User)
            .WithMany()
            .HasForeignKey(enrollment => enrollment.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
