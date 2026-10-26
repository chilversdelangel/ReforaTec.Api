using ReforaTec.Api.Database;
using ReforaTec.Api.Infrastructure.Endpoints;
using ReforaTec.Api.Infrastructure.Filters;
using ReforaTec.Api.Infrastructure.Mapping;
using ReforaTec.Api.Infrastructure.OpenApi;
using ReforaTec.Api.Infrastructure.Security.Otp;
using ReforaTec.Api.Infrastructure.Security.RateLimiting;

namespace ReforaTec.Api.Features.Auth.RequestOtp;

internal sealed class RequestOtp : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/otp-codes", HandleRequest)
            .WithTags(OpenApiTags.Auth)
            .AllowAnonymous()
            .RequireRateLimiting(RateLimitingPolicies.Otp)
            .AddEndpointFilter<OtpRateLimitingFilter>()
            .AddEndpointFilter<ValidationFilter<Request>>()
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .WithSummary("Request login OTP code")
            .WithDescription("Sends a one-time password code to the registered email address if it exists.");
    }

    private static async Task<IResult> HandleRequest(
        Request request,
        AppDbContext context,
        IOtpService otpService,
        CancellationToken cancellationToken)
    {
        var result = await Handler.Handle(request, context, otpService, cancellationToken);

        return result.Match(
            success => Results.NoContent(),
            errors => errors.ToProblem());
    }
}