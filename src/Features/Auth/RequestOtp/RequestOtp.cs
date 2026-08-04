using ReforaTec.Api.Database;
using ReforaTec.Api.Infrastructure.Filters;
using ReforaTec.Api.Infrastructure.Mapping;
using ReforaTec.Api.Infrastructure.Security.Otp;

namespace ReforaTec.Api.Features.Auth.RequestOtp;

public static class RequestOtp
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/otp-codes", HandleRequest)
            .AllowAnonymous()
            .AddEndpointFilter<ValidationFilter<Request>>()
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> HandleRequest(
        Request request, 
        AppDbContext context, 
        IOtpService otpService,
        CancellationToken cancellationToken)
    {
        var result = await Handler.Handle(request, context, otpService, cancellationToken);

        return result.Match(
            _ => Results.Ok(new { Message = "If the email is registered, an OTP code has been sent." }),
            errors => errors.ToProblem());
    }
}
