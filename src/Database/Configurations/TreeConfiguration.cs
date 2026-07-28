using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReforaTec.Api.Entities;

namespace ReforaTec.Api.Database.Configurations;

public class TreeConfiguration : IEntityTypeConfiguration<Tree>
{
    public void Configure(EntityTypeBuilder<Tree> builder)
    {
        builder.OwnsOne(t => t.Location);

        builder.Property(t => t.HeightCentimeters)
            .HasPrecision(7, 2);

        builder.Property(t => t.DiameterCentimeters)
            .HasPrecision(7, 2);

        builder.Property(t => t.Notes)
            .HasMaxLength(500);

        builder.HasIndex(t => t.TenantId);
        builder.HasIndex(t => t.SpeciesId);
        builder.HasIndex(t => t.ValueId);

        builder.HasOne(t => t.Tenant)
            .WithMany()
            .HasForeignKey(t => t.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Species)
            .WithMany()
            .HasForeignKey(t => t.SpeciesId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Value)
            .WithMany()
            .HasForeignKey(t => t.ValueId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}