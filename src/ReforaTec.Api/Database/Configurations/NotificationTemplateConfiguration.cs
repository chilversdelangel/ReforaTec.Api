using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReforaTec.Api.Entities;

namespace ReforaTec.Api.Database.Configurations;

public class NotificationTemplateConfiguration : IEntityTypeConfiguration<NotificationTemplate>
{
    public void Configure(EntityTypeBuilder<NotificationTemplate> builder)
    {
        builder.Property(n => n.Title)
            .HasMaxLength(150);

        builder.Property(n => n.MessageBody)
            .HasMaxLength(500);

        builder.HasIndex(n => n.Type);
    }
}
