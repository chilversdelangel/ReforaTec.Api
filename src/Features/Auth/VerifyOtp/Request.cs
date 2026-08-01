namespace ReforaTec.Api.Features.Auth.VerifyOtp;

public record Request(
    string Email, 
    string OtpCode, 
    string Audience);
