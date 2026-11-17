using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReforaTec.Api.Entities;

namespace ReforaTec.Api.Database.Configurations;

public class VirtueConfiguration : IEntityTypeConfiguration<Virtue>
{
    public void Configure(EntityTypeBuilder<Virtue> builder)
    {
        builder.Property(virtue => virtue.Name)
            .IsRequired()
            .HasMaxLength(25);

        builder.Property(virtue => virtue.NormalizedName)
            .IsRequired()
            .HasMaxLength(25);

        builder.HasIndex(virtue => virtue.NormalizedName)
            .IsUnique();
    }
}
