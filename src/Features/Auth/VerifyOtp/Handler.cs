using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ReforaTec.Api.Common.Helpers;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities;
using ReforaTec.Api.Entities.Enums;
using ReforaTec.Api.Infrastructure.Security.Jwt;

namespace ReforaTec.Api.Features.Auth.VerifyOtp;

public static class Handler
{
    public static async Task<ErrorOr<Response>> Handle(
        Request request,
        AppDbContext context,
        IJwtTokenService jwtTokenService,
        IOptionsSnapshot<JwtOptions> jwtOptions,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.ToNormalized();

        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail && !u.IsDeleted, cancellationToken);

        if (user is null)
            return Error.Unauthorized(ErrorCodes.InvalidCredentials, "Invalid email or OTP code.");

        var otpCode = await context.AuthOtpCodes
            .FirstOrDefaultAsync(o => o.UserId == user.Id, cancellationToken);

        if (otpCode is null)
            return Error.Unauthorized(ErrorCodes.InvalidCredentials, "Invalid email or OTP code.");

        // In-memory OTP rule check
        var otpValidationResult = ValidateOtp(otpCode, request.OtpCode);

        if (otpValidationResult.IsError)
        {
            await UpdateOtpStateAsync(context, otpCode, otpValidationResult.FirstError, cancellationToken);
            return otpValidationResult.Errors;
        }
        
        context.AuthOtpCodes.Remove(otpCode);
        
        await EnforceMaxSessionsAsync(context, user.Id, user.CurrentRole, jwtOptions, cancellationToken);

        var accessToken = jwtTokenService.GenerateAccessToken(
            new TokenGenerationRequest(user.Id, user.CurrentRole, request.Audience));

        var rawRefreshToken = jwtTokenService.GenerateRawRefreshToken();
        var hashedRefreshToken = jwtTokenService.HashRefreshToken(rawRefreshToken);
        context.RefreshTokens.Add(
            CreateRefreshTokenEntity(user.Id, hashedRefreshToken, request.Audience));

        await context.SaveChangesAsync(cancellationToken);

        return new Response(accessToken, rawRefreshToken);
    }

    private static ErrorOr<Success> ValidateOtp(AuthOtpCode otpCode, string inputOtpCode)
    {
        if (otpCode.ExpiresAt < DateTime.UtcNow)
            return Error.Unauthorized(ErrorCodes.OtpExpired, "OTP code has expired. Please request a new one.");

        if (otpCode.FailedAttempts >= AuthOtpCode.MaxFailedAttempts)
            return Error.Unauthorized(ErrorCodes.TooManyAttempts, "Too many failed attempts. Please request a new OTP code.");

        if (otpCode.VerificationCode != inputOtpCode)
        {
            otpCode.FailedAttempts++;

            return otpCode.FailedAttempts >= AuthOtpCode.MaxFailedAttempts
                ? Error.Unauthorized(ErrorCodes.TooManyAttempts, "Too many failed attempts. OTP code destroyed.")
                : Error.Unauthorized(ErrorCodes.InvalidCredentials, "Invalid email or OTP code.");
        }

        return Result.Success;
    }

    private static async Task UpdateOtpStateAsync(
        AppDbContext context,
        AuthOtpCode otpRecord,
        Error error,
        CancellationToken cancellationToken)
    {
        if (error.Code is ErrorCodes.OtpExpired or ErrorCodes.TooManyAttempts)
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
            .Where(r => r.UserId == userId)
            .OrderBy(r => r.CreatedAt)
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