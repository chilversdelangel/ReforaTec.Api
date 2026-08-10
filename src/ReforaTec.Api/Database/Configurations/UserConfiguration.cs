using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReforaTec.Api.Entities;

namespace ReforaTec.Api.Database.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.MiddleName)
            .HasMaxLength(50);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.SecondLastName)
            .HasMaxLength(50);

        builder.Property(u => u.ControlNumber)
            .HasMaxLength(30);

        builder.Property(u => u.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(u => u.CurrentRole)
            .IsRequired();

        builder.HasIndex(u => u.Email)
            .IsUnique();

        // Multi-Tenant Composite Unique Index: Enforces control number uniqueness per institution,
        // while allowing different institutions to have independent control numbers. NULL values are ignored.
        builder.HasIndex(u => new { u.TenantId, u.ControlNumber })
            .IsUnique();

        builder.HasOne(u => u.Tenant)
            .WithMany()
            .HasForeignKey(u => u.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        // Note: Soft delete (IsDeleted) is handled explicitly in application feature handlers
        // rather than via global HasQueryFilter to allow historical audit logs to include user navigation properties.
    }
}
