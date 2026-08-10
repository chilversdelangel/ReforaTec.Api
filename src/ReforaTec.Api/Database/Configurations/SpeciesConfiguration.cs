using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReforaTec.Api.Entities;

namespace ReforaTec.Api.Database.Configurations;

public class SpeciesConfiguration : IEntityTypeConfiguration<Species>
{
    public void Configure(EntityTypeBuilder<Species> builder)
    {
        builder.Property(s => s.ScientificName)
            .HasMaxLength(100);

        builder.Property(s => s.NormalizedScientificName)
            .HasMaxLength(100);

        builder.Property(s => s.CommonName)
            .HasMaxLength(100);

        builder.Property(s => s.NormalizedCommonName)
            .HasMaxLength(100);

        builder.Property(s => s.Description)
            .HasMaxLength(1000);

        builder.Property(s => s.ImageUrl)
            .HasMaxLength(2048);

        // Unique Index: Guarantees strict botanical uniqueness per scientific binomial name
        builder.HasIndex(s => s.NormalizedScientificName)
            .IsUnique();

        // Non-Unique Index: Enables fast O(log N) mobile search by common name
        // while allowing shared common names between different tree species.
        builder.HasIndex(s => s.NormalizedCommonName);
    }
}