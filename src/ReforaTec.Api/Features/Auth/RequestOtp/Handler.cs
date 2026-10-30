using ErrorOr;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Common.Helpers;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities;
using ReforaTec.Api.Infrastructure.Email;
using ReforaTec.Api.Infrastructure.Security.Otp;

namespace ReforaTec.Api.Features.Auth.RequestOtp;

internal static class Handler
{
    public static async Task<ErrorOr<Success>> Handle(
        Request request,
        AppDbContext context,
        IOtpService otpService,
        IEmailQueue emailQueue,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.ToNormalized();

        var user = await context.Users
            .FirstOrDefaultAsync(user => user.Email == normalizedEmail && !user.IsDeleted, cancellationToken);

        // OWASP User Enumeration Prevention: Do not leak user existence
        if (user is null)
        {
            return Result.Success;
        }

        var otpCode = otpService.GenerateOtp();
        var expiresAt = DateTime.UtcNow.AddMinutes(5);

        var existingOtp = await context.AuthOtpCodes
            .FirstOrDefaultAsync(otp => otp.UserId == user.Id, cancellationToken);

        if (existingOtp is not null)
        {
            existingOtp.VerificationCode = otpCode;
            existingOtp.FailedAttempts = 0;
            existingOtp.ExpiresAt = expiresAt;
        }
        else
        {
            context.AuthOtpCodes.Add(new AuthOtpCode
            {
                UserId = user.Id,
                VerificationCode = otpCode,
                FailedAttempts = 0,
                ExpiresAt = expiresAt
            });
        }

        await context.SaveChangesAsync(cancellationToken);

        var email = EmailTemplates.CreateOtpEmail(user.Email, otpCode);
        await emailQueue.EnqueueAsync(email, cancellationToken);

        return Result.Success;
    }
}