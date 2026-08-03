using ErrorOr;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Common.Helpers;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities;
using ReforaTec.Api.Infrastructure.Security.Jwt;

namespace ReforaTec.Api.Features.Auth.VerifyOtp;

public static class Handler
{
    public static async Task<ErrorOr<Response>> Handle(
        Request request,
        AppDbContext context,
        IJwtTokenService jwtTokenService,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.ToNormalized();

        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail && !u.IsDeleted, cancellationToken);

        if (user is null)
            return Error.Unauthorized(ErrorCodes.InvalidCredentials, "Invalid email or OTP code.");

        var persistedOtpCode = await context.AuthOtpCodes
            .FirstOrDefaultAsync(o => o.UserId == user.Id, cancellationToken);

        if (persistedOtpCode is null)
            return Error.Unauthorized(ErrorCodes.InvalidCredentials, "Invalid email or OTP code.");

        // In-memory OTP rule check
        var otpValidationResult = ValidateOtp(persistedOtpCode, request.OtpCode);

        if (otpValidationResult.IsError)
        {
            await UpdateOtpStateAsync(context, persistedOtpCode, otpValidationResult.FirstError, cancellationToken);
            return otpValidationResult.Errors;
        }

        context.AuthOtpCodes.Remove(persistedOtpCode);

        var accessToken = jwtTokenService.GenerateAccessToken(
            new TokenGenerationRequest(user.Id, user.CurrentRole, request.Audience));

        var refreshTokenResult = jwtTokenService.GenerateRefreshToken();
        context.RefreshTokens.Add(
            CreateRefreshTokenEntity(user.Id, refreshTokenResult.HashedToken, request.Audience));

        await context.SaveChangesAsync(cancellationToken);

        return new Response(accessToken, refreshTokenResult.RawToken);
    }

    private static ErrorOr<Success> ValidateOtp(AuthOtpCode persistedOtpCode, string inputOtpCode)
    {
        if (persistedOtpCode.ExpiresAt < DateTime.UtcNow)
            return Error.Unauthorized(
                ErrorCodes.OtpExpired,
                "OTP code has expired. Please request a new one.");

        if (persistedOtpCode.FailedAttempts >= AuthOtpCode.MaxFailedAttempts)
            return Error.Unauthorized(
                ErrorCodes.TooManyAttempts,
                "Too many failed attempts. Please request a new OTP code.");

        if (persistedOtpCode.VerificationCode == inputOtpCode) return Result.Success;
        persistedOtpCode.FailedAttempts++;

        return persistedOtpCode.FailedAttempts >= AuthOtpCode.MaxFailedAttempts
            ? Error.Unauthorized(ErrorCodes.TooManyAttempts, "Too many failed attempts. OTP code destroyed.")
            : Error.Unauthorized(ErrorCodes.InvalidCredentials, "Invalid email or OTP code.");
    }

    private static async Task UpdateOtpStateAsync(
        AppDbContext context,
        AuthOtpCode otpRecord,
        Error error,
        CancellationToken cancellationToken)
    {
        if (error.Code is ErrorCodes.OtpExpired or ErrorCodes.TooManyAttempts)
            context.AuthOtpCodes.Remove(otpRecord);

        await context.SaveChangesAsync(cancellationToken);
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