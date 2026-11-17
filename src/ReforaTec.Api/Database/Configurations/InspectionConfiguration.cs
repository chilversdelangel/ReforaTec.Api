using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReforaTec.Api.Entities;

namespace ReforaTec.Api.Database.Configurations;

internal sealed class InspectionConfiguration : IEntityTypeConfiguration<Inspection>
{
    public void Configure(EntityTypeBuilder<Inspection> builder)
    {
        builder.Property(inspection => inspection.HealthState)
            .IsRequired();

        builder.Property(inspection => inspection.Notes)
            .HasMaxLength(500);

        builder.HasIndex(inspection => inspection.TenantId);
        builder.HasIndex(inspection => inspection.TreeId);
        builder.HasIndex(inspection => inspection.UserId);

        builder.HasOne(inspection => inspection.Tenant)
            .WithMany()
            .HasForeignKey(inspection => inspection.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(inspection => inspection.Tree)
            .WithMany()
            .HasForeignKey(inspection => inspection.TreeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(inspection => inspection.User)
            .WithMany()
            .HasForeignKey(inspection => inspection.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
