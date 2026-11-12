using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReforaTec.Api.Entities;

namespace ReforaTec.Api.Database.Configurations;

internal sealed class CareAssignmentConfiguration : IEntityTypeConfiguration<CareAssignment>
{
    public void Configure(EntityTypeBuilder<CareAssignment> builder)
    {
        builder.ToTable(tableBuilder =>
            tableBuilder.HasCheckConstraint(
                "ck_care_assignments_end_date_after_start_date",
                "end_date IS NULL OR end_date >= start_date"));

        builder.HasIndex(assignment => assignment.TenantId);
        builder.HasIndex(assignment => assignment.CampaignId);
        builder.HasIndex(assignment => assignment.UserId);
        builder.HasIndex(assignment => assignment.TreeId);

        builder.HasIndex(assignment => new { assignment.CampaignId, assignment.UserId, assignment.TreeId })
            .IsUnique()
            .HasFilter("end_date IS NULL");

        builder.HasOne(assignment => assignment.Tenant)
            .WithMany()
            .HasForeignKey(assignment => assignment.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(assignment => assignment.Campaign)
            .WithMany()
            .HasForeignKey(assignment => assignment.CampaignId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(assignment => assignment.User)
            .WithMany()
            .HasForeignKey(assignment => assignment.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(assignment => assignment.Tree)
            .WithMany()
            .HasForeignKey(assignment => assignment.TreeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
