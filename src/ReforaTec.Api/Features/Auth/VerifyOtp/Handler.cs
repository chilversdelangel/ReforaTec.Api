using ErrorOr;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Common.Helpers;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities;
using ReforaTec.Api.Infrastructure.Security.Jwt;
using ReforaTec.Api.Infrastructure.Security.Otp;

namespace ReforaTec.Api.Features.Auth.VerifyOtp;

internal static class Handler
{
    public static async Task<ErrorOr<Response>> Handle(
        Request request,
        AppDbContext context,
        IJwtService jwtService,
        JwtOptions jwtOptions,
        OtpOptions otpOptions,
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

        var persistedOtp = await context.OneTimePasswords
            .FirstOrDefaultAsync(otp => otp.UserId == user.Id, cancellationToken);

        if (persistedOtp is null)
        {
            return Errors.InvalidCredentials;
        }

        // In-memory OTP rule check
        var otpValidationResult = 
            ValidateOtp(persistedOtp, request.OtpCode, otpOptions.MaxFailedAttempts, env, now);

        if (otpValidationResult.IsError)
        {
            await UpdateOtpStateAsync(context, persistedOtp, otpValidationResult.FirstError, cancellationToken);
            return otpValidationResult.Errors;
        }
        
        context.OneTimePasswords.Remove(persistedOtp);
        
        if (!user.IsEmailVerified)
        {
            user.IsEmailVerified = true;
        }
        
        var maxAllowedSessions = jwtOptions.GetMaxSessionsForRole(user.Role);
        await EnforceMaxSessionsAsync(context, user.Id, maxAllowedSessions, cancellationToken);

        var accessToken = jwtService.GenerateAccessToken(
            new TokenGenerationRequest(user.Id, user.TenantId, user.Role, request.Audience));

        var refreshToken = jwtService.GenerateRefreshToken(now);
        context.RefreshTokens.Add(
            CreateRefreshTokenEntity(user.Id, refreshToken.HashedToken, request.Audience, refreshToken.ExpiresAt));

        await context.SaveChangesAsync(cancellationToken);

        var expiresInSeconds = jwtOptions.AccessTokenExpirationHours * 3600;

        return new Response(accessToken, refreshToken.RawToken, "Bearer", expiresInSeconds);
    }

    private static ErrorOr<Success> ValidateOtp(
        OneTimePassword persistedOtp,
        string requestedOtpCode,
        int maxFailedAttempts,
        IWebHostEnvironment env,
        DateTime now)
    {
        // QA OTP Backdoor
        if (!env.IsProduction() && requestedOtpCode == "000000")
        {
            return Result.Success;
        }

        if (persistedOtp.ExpiresAt < now)
        {
            return Errors.OtpExpired;
        }

        if (persistedOtp.FailedAttempts >= maxFailedAttempts)
        {
            return Errors.TooManyAttempts;
        }

        if (persistedOtp.Code != requestedOtpCode)
        {
            persistedOtp.FailedAttempts++;

            return persistedOtp.FailedAttempts >= maxFailedAttempts
                ? Errors.TooManyAttemptsDestroyed
                : Errors.InvalidCredentials;
        }

        return Result.Success;
    }

    private static async Task UpdateOtpStateAsync(
        AppDbContext context,
        OneTimePassword persistedOtp,
        Error error,
        CancellationToken cancellationToken)
    {
        if (error == Errors.OtpExpired || error == Errors.TooManyAttempts)
        {
            context.OneTimePasswords.Remove(persistedOtp);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnforceMaxSessionsAsync(
        AppDbContext context,
        int userId,
        int maxAllowedSessions,
        CancellationToken cancellationToken)
    {
        var activeSessions = await context.RefreshTokens
            .Where(refreshToken => refreshToken.UserId == userId)
            .OrderBy(refreshToken => refreshToken.CreatedAt)
            .ToListAsync(cancellationToken);

        if (activeSessions.Count >= maxAllowedSessions)
        {
            var sessionsToRemoveCount = activeSessions.Count - maxAllowedSessions + 1;
            var oldestSessions = activeSessions.Take(sessionsToRemoveCount);
            context.RefreshTokens.RemoveRange(oldestSessions);
        }
    }

    private static RefreshToken CreateRefreshTokenEntity(
        int userId,
        string hashedToken,
        string audience,
        DateTime expiresAt) =>
        new()
        {
            UserId = userId,
            HashedToken = hashedToken,
            Audience = audience,
            ExpiresAt = expiresAt
        };
}