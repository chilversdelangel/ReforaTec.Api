using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReforaTec.Api.Entities;

namespace ReforaTec.Api.Database.Configurations;

public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.Property(s => s.Comment)
            .HasMaxLength(500);

        builder.HasIndex(s => s.TenantId);
        builder.HasIndex(s => s.StudentId);
        builder.HasIndex(s => s.TreeId);
        builder.HasIndex(s => s.CampaignId);

        builder.HasOne(s => s.Tenant)
            .WithMany()
            .HasForeignKey(s => s.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Student)
            .WithMany()
            .HasForeignKey(s => s.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Tree)
            .WithMany()
            .HasForeignKey(s => s.TreeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.ServiceType)
            .WithMany()
            .HasForeignKey(s => s.ServiceTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Campaign)
            .WithMany()
            .HasForeignKey(s => s.CampaignId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
