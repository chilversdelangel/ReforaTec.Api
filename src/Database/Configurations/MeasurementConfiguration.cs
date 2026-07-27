using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReforaTec.Api.Entities;

namespace ReforaTec.Api.Database.Configurations;

public class MeasurementConfiguration : IEntityTypeConfiguration<Measurement>
{
    public void Configure(EntityTypeBuilder<Measurement> builder)
    {
        builder.Property(m => m.InspectionType)
            .IsRequired();

        builder.Property(m => m.DetectedHealthState)
            .IsRequired();

        builder.Property(m => m.HeightCentimeters)
            .HasPrecision(7, 2);

        builder.Property(m => m.DiameterCentimeters)
            .HasPrecision(7, 2);

        builder.Property(m => m.EvidencePhotoUrl)
            .HasMaxLength(300);

        builder.Property(m => m.ObservationNotes)
            .HasMaxLength(500);

        builder.HasIndex(m => m.TenantId);
        builder.HasIndex(m => m.TreeId);
        builder.HasIndex(m => m.InspectorId);
        builder.HasIndex(m => m.CampaignId);

        builder.HasOne(m => m.Tenant)
            .WithMany()
            .HasForeignKey(m => m.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Tree)
            .WithMany()
            .HasForeignKey(m => m.TreeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Inspector)
            .WithMany()
            .HasForeignKey(m => m.InspectorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Campaign)
            .WithMany()
            .HasForeignKey(m => m.CampaignId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
