using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReforaTec.Api.Entities;

namespace ReforaTec.Api.Database.Configurations;

public class AuthOtpCodeConfiguration : IEntityTypeConfiguration<AuthOtpCode>
{
    public void Configure(EntityTypeBuilder<AuthOtpCode> builder)
    {
        builder.HasKey(a => a.UserId);

        builder.Property(a => a.VerificationCode)
            .IsRequired()
            .HasMaxLength(10);

        builder.HasOne(a => a.User)
            .WithOne()
            .HasForeignKey<AuthOtpCode>(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
