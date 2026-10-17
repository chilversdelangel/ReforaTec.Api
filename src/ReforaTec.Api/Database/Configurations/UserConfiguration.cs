using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReforaTec.Api.Entities;

namespace ReforaTec.Api.Database.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(user => user.Email)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(user => user.GivenName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(user => user.DisplayName)
            .HasMaxLength(50);

        builder.Property(user => user.FamilyName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(user => user.SecondFamilyName)
            .HasMaxLength(50);

        builder.Property(user => user.ControlNumber)
            .HasMaxLength(30);

        builder.Property(user => user.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(user => user.CurrentRole)
            .IsRequired();

        builder.HasIndex(user => user.Email)
            .IsUnique();

        // Multi-Tenant Composite Unique Index: Enforces control number uniqueness per institution,
        // while allowing different institutions to have independent control numbers. NULL values are ignored.
        builder.HasIndex(user => new { user.TenantId, user.ControlNumber })
            .IsUnique();

        builder.HasOne(user => user.Tenant)
            .WithMany()
            .HasForeignKey(user => user.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        // Note: Soft delete (IsDeleted) is handled explicitly in application feature handlers
        // rather than via global HasQueryFilter to allow historical audit logs to include user navigation properties.
    }
}
