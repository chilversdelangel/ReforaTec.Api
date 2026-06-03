using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReforaTec.Api.Entities;

namespace ReforaTec.Api.Database.Configurations;

public class SpeciesConfiguration : IEntityTypeConfiguration<Species>
{
    public void Configure(EntityTypeBuilder<Species> builder)
    {
        builder.Property(s => s.ScientificName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.NormalizedScientificName)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(s => s.NormalizedScientificName)
            .IsUnique();

        builder.Property(s => s.CommonNames)
            .IsRequired()
            .HasColumnType("varchar(50)[]")
            .HasDefaultValueSql("'{}'::varchar(50)[]");

        builder.Property(s => s.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(s => s.ImageUrl)
            .HasMaxLength(2048);
    }
}