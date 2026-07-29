using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReforaTec.Api.Entities;

namespace ReforaTec.Api.Database.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.Property(t => t.InstitutionName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(t => t.NormalizedInstitutionName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(t => t.Acronym)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(t => t.InstitutionalEmailDomain)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(t => t.NormalizedInstitutionName)
            .IsUnique();

        builder.HasIndex(t => t.InstitutionalEmailDomain)
            .IsUnique();

        // Note: Soft delete (IsDeleted) is handled explicitly in application feature handlers
        // rather than via global HasQueryFilter to allow historical audit logs to include tenant navigation properties.
    }
}
