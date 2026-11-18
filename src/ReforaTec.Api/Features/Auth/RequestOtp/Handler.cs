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
        OtpOptions otpOptions,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var normalizedEmail = request.Email.ToNormalized();

        var user = await context.Users
            .FirstOrDefaultAsync(user => user.Email == normalizedEmail, cancellationToken);

        // OWASP User Enumeration Prevention: Do not leak user existence
        if (user is null)
        {
            return Result.Success;
        }

        var otpCode = otpService.GenerateOtp();
        var expiresAt = now.AddMinutes(otpOptions.ExpirationMinutes);

        var existingOtp = await context.OneTimePasswords
            .FirstOrDefaultAsync(otp => otp.UserId == user.Id, cancellationToken);

        if (existingOtp is not null)
        {
            existingOtp.Code = otpCode;
            existingOtp.FailedAttempts = 0;
            existingOtp.ExpiresAt = expiresAt;
        }
        else
        {
            context.OneTimePasswords.Add(new OneTimePassword
            {
                UserId = user.Id,
                Code = otpCode,
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