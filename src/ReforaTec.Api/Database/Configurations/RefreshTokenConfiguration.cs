using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReforaTec.Api.Entities;

namespace ReforaTec.Api.Database.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("auth_refresh_tokens");

        builder.Property(rt => rt.HashedToken)
            .HasMaxLength(64)
            .IsFixedLength(); // SHA-256 in hex is always 64 characters

        builder.Property(rt => rt.Audience)
            .HasMaxLength(100);

        builder.HasOne(rt => rt.User)
            .WithMany()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // O(1) Lookups when receiving a refresh token
        builder.HasIndex(rt => rt.HashedToken)
            .IsUnique();

        // Optimized query for MaxSessionsPerRole (finding the oldest session for a user)
        builder.HasIndex(rt => new { rt.UserId, rt.CreatedAt });

        // Optimization for Background Job (Garbage Collection of expired tokens)
        builder.HasIndex(rt => rt.ExpiresAt);
    }
}
