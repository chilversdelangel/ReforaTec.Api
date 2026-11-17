using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReforaTec.Api.Entities;

namespace ReforaTec.Api.Database.Configurations;

internal sealed class CareLogConfiguration : IEntityTypeConfiguration<CareLog>
{
    public void Configure(EntityTypeBuilder<CareLog> builder)
    {
        builder.Property(careLog => careLog.Notes)
            .HasMaxLength(500);

        builder.HasIndex(careLog => careLog.TenantId);
        builder.HasIndex(careLog => careLog.UserId);
        builder.HasIndex(careLog => careLog.TreeId);
        builder.HasIndex(careLog => careLog.CareTypeId);

        builder.HasOne(careLog => careLog.Tenant)
            .WithMany()
            .HasForeignKey(careLog => careLog.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(careLog => careLog.User)
            .WithMany()
            .HasForeignKey(careLog => careLog.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(careLog => careLog.Tree)
            .WithMany()
            .HasForeignKey(careLog => careLog.TreeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(careLog => careLog.CareType)
            .WithMany()
            .HasForeignKey(careLog => careLog.CareTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
