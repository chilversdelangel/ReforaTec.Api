using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReforaTec.Api.Entities;

namespace ReforaTec.Api.Database.Configurations;

internal sealed class TreeConfiguration : IEntityTypeConfiguration<Tree>
{
    public void Configure(EntityTypeBuilder<Tree> builder)
    {
        builder.OwnsOne(tree => tree.Location);

        builder.Property(tree => tree.Folio)
            .HasMaxLength(10);

        builder.Property(tree => tree.NormalizedFolio)
            .HasMaxLength(10);

        builder.HasIndex(tree => tree.TenantId);
        builder.HasIndex(tree => tree.SpeciesId);
        builder.HasIndex(tree => tree.VirtueId);

        builder.HasIndex(tree => new { tree.TenantId, tree.NormalizedFolio })
            .IsUnique();

        builder.HasOne(tree => tree.Tenant)
            .WithMany()
            .HasForeignKey(tree => tree.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(tree => tree.Species)
            .WithMany()
            .HasForeignKey(tree => tree.SpeciesId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(tree => tree.Virtue)
            .WithMany()
            .HasForeignKey(tree => tree.VirtueId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}