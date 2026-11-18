using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ReforaTec.Api.Common.Helpers;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities;
using ReforaTec.Api.Entities.Enums;
using ReforaTec.Api.Infrastructure.Security.Jwt;

namespace ReforaTec.Api.Features.Auth.VerifyOtp;

internal static class Handler
{
    public static async Task<ErrorOr<Response>> Handle(
        Request request,
        AppDbContext context,
        IJwtTokenService jwtTokenService,
        IOptionsSnapshot<JwtOptions> jwtOptions,
        IWebHostEnvironment env,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var normalizedEmail = request.Email.ToNormalized();

        var user = await context.Users
            .FirstOrDefaultAsync(user => user.Email == normalizedEmail, cancellationToken);

        if (user is null)
        {
            return Errors.InvalidCredentials;
        }

        var persistedOtpCode = await context.AuthOtpCodes
            .FirstOrDefaultAsync(otp => otp.UserId == user.Id, cancellationToken);

        if (persistedOtpCode is null)
        {
            return Errors.InvalidCredentials;
        }

        // In-memory OTP rule check
        var otpValidationResult = ValidateOtp(persistedOtpCode, request.OtpCode, env, now);

        if (otpValidationResult.IsError)
        {
            await UpdateOtpStateAsync(context, persistedOtpCode, otpValidationResult.FirstError, cancellationToken);
            return otpValidationResult.Errors;
        }
        
        context.AuthOtpCodes.Remove(persistedOtpCode);
        
        if (!user.IsEmailVerified)
        {
            user.IsEmailVerified = true;
        }
        
        await EnforceMaxSessionsAsync(context, user.Id, user.Role, jwtOptions, cancellationToken);

        var accessToken = jwtTokenService.GenerateAccessToken(
            new TokenGenerationRequest(user.Id, user.TenantId, user.Role, request.Audience));

        var rawRefreshToken = jwtTokenService.GenerateRawRefreshToken();
        var hashedRefreshToken = jwtTokenService.HashRefreshToken(rawRefreshToken);
        context.RefreshTokens.Add(
            CreateRefreshTokenEntity(user.Id, hashedRefreshToken, request.Audience));

        await context.SaveChangesAsync(cancellationToken);

        var expiresInSeconds = jwtOptions.Value.AccessTokenExpirationHours * 3600;

        return new Response(accessToken, rawRefreshToken, "Bearer", expiresInSeconds);
    }

    private static ErrorOr<Success> ValidateOtp(
        AuthOtpCode persistedOtpCode,
        string requestedOtpCode,
        IWebHostEnvironment env,
        DateTime now)
    {
        // QA OTP Backdoor
        if (!env.IsProduction() && requestedOtpCode == "000000")
        {
            return Result.Success;
        }

        if (persistedOtpCode.ExpiresAt < now)
        {
            return Errors.OtpExpired;
        }

        if (persistedOtpCode.FailedAttempts >= AuthOtpCode.MaxFailedAttempts)
        {
            return Errors.TooManyAttempts;
        }

        if (persistedOtpCode.VerificationCode != requestedOtpCode)
        {
            persistedOtpCode.FailedAttempts++;

            return persistedOtpCode.FailedAttempts >= AuthOtpCode.MaxFailedAttempts
                ? Errors.TooManyAttemptsDestroyed
                : Errors.InvalidCredentials;
        }

        return Result.Success;
    }

    private static async Task UpdateOtpStateAsync(
        AppDbContext context,
        AuthOtpCode otpRecord,
        Error error,
        CancellationToken cancellationToken)
    {
        if (error == Errors.OtpExpired || error == Errors.TooManyAttempts)
        {
            context.AuthOtpCodes.Remove(otpRecord);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnforceMaxSessionsAsync(
        AppDbContext context,
        int userId,
        UserRole role,
        IOptionsSnapshot<JwtOptions> jwtOptions,
        CancellationToken cancellationToken)
    {
        var roleKey = role.ToString();
        var maxAllowed = jwtOptions.Value.MaxSessionsPerRole.GetValueOrDefault(roleKey, 2);

        var activeSessions = await context.RefreshTokens
            .Where(refreshToken => refreshToken.UserId == userId)
            .OrderBy(refreshToken => refreshToken.CreatedAt)
            .ToListAsync(cancellationToken);

        if (activeSessions.Count >= maxAllowed)
        {
            var sessionsToRemoveCount = activeSessions.Count - maxAllowed + 1;
            var oldestSessions = activeSessions.Take(sessionsToRemoveCount);
            context.RefreshTokens.RemoveRange(oldestSessions);
        }
    }

    private static RefreshToken CreateRefreshTokenEntity(int userId, string hashedToken, string audience) =>
        new()
        {
            UserId = userId,
            HashedToken = hashedToken,
            Audience = audience,
            ExpiresAt = DateTime.UtcNow.AddDays(21)
        };
}