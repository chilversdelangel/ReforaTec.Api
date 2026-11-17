using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReforaTec.Api.Entities;

namespace ReforaTec.Api.Database.Configurations;

internal sealed class CareTypeConfiguration : IEntityTypeConfiguration<CareType>
{
    public void Configure(EntityTypeBuilder<CareType> builder)
    {
        builder.Property(careType => careType.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(careType => careType.NormalizedName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(careType => careType.IconUrl)
            .HasMaxLength(300);

        builder.HasIndex(careType => careType.NormalizedName)
            .IsUnique();
    }
}
