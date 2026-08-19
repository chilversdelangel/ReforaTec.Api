using ReforaTec.Api.Database;
using ReforaTec.Api.Infrastructure.Endpoints;
using ReforaTec.Api.Infrastructure.Filters;
using ReforaTec.Api.Infrastructure.Mapping;
using ReforaTec.Api.Infrastructure.Security.Otp;

namespace ReforaTec.Api.Features.Auth.RequestOtp;

internal sealed class RequestOtp : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/otp-codes", HandleRequest)
            .AllowAnonymous()
            .AddEndpointFilter<ValidationFilter<Request>>()
            .Produces<Response>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .WithSummary("Request login OTP code")
            .WithDescription("Sends an OTP code (and returns it in development) for user login.");
    }

    private static async Task<IResult> HandleRequest(
        Request request,
        AppDbContext context,
        IOtpService otpService,
        CancellationToken cancellationToken)
    {
        var result = await Handler.Handle(request, context, otpService, cancellationToken);

        return result.Match(
            response => Results.Ok(response),
            errors => errors.ToProblem());
    }
}