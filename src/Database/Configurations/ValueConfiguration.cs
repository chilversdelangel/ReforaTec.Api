using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReforaTec.Api.Entities;

namespace ReforaTec.Api.Database.Configurations;

public class ValueConfiguration : IEntityTypeConfiguration<Value>
{
    public void Configure(EntityTypeBuilder<Value> builder)
    {
        builder.Property(v => v.ValueName)
            .IsRequired()
            .HasMaxLength(25);

        builder.Property(v => v.NormalizedValueName)
            .IsRequired()
            .HasMaxLength(25);

        builder.HasIndex(v => v.NormalizedValueName)
            .IsUnique();
    }
}