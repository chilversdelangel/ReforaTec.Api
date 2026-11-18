using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReforaTec.Api.Entities;

namespace ReforaTec.Api.Database.Configurations;

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.Property(refreshToken => refreshToken.HashedToken)
            .HasMaxLength(64)
            .IsFixedLength(); // SHA-256 in hex is always 64 characters

        builder.Property(refreshToken => refreshToken.Audience)
            .HasMaxLength(100);

        builder.HasOne(refreshToken => refreshToken.User)
            .WithMany()
            .HasForeignKey(refreshToken => refreshToken.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // O(1) Lookups when receiving a refresh token
        builder.HasIndex(refreshToken => refreshToken.HashedToken)
            .IsUnique();

        // Optimized query for MaxSessionsPerRole (finding the oldest session for a user)
        builder.HasIndex(refreshToken => new { refreshToken.UserId, refreshToken.CreatedAt });

        // Optimization for Background Job (Garbage Collection of expired tokens)
        builder.HasIndex(refreshToken => refreshToken.ExpiresAt);
    }
}
