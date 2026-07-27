using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReforaTec.Api.Entities;

namespace ReforaTec.Api.Database.Configurations;

public class ServiceTypeConfiguration : IEntityTypeConfiguration<ServiceType>
{
    public void Configure(EntityTypeBuilder<ServiceType> builder)
    {
        builder.Property(s => s.ServiceName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.NormalizedServiceName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.IconUrl)
            .HasMaxLength(300);

        builder.HasIndex(s => s.NormalizedServiceName)
            .IsUnique();
    }
}
