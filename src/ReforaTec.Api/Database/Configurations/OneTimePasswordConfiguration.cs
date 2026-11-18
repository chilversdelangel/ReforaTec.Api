using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReforaTec.Api.Entities;

namespace ReforaTec.Api.Database.Configurations;

internal sealed class OneTimePasswordConfiguration : IEntityTypeConfiguration<OneTimePassword>
{
    public void Configure(EntityTypeBuilder<OneTimePassword> builder)
    {
        builder.HasKey(otp => otp.UserId);

        builder.Property(otp => otp.Code)
            .IsRequired()
            .HasMaxLength(10);

        builder.HasOne(otp => otp.User)
            .WithOne()
            .HasForeignKey<OneTimePassword>(otp => otp.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
