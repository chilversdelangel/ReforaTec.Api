using FluentValidation;

namespace ReforaTec.Api.Infrastructure.Filters;

internal sealed class ValidationFilter<T> : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var requestObject = context.Arguments.OfType<T>().FirstOrDefault();
        
        if (requestObject is null)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Bad Request",
                detail: "The request body is missing, empty, or could not be deserialized.");
        }

        var validator = context.HttpContext.RequestServices.GetService<IValidator<T>>();
       
        if (validator is null)
        {
            return await next(context);
        }

        var validateResult = await validator.ValidateAsync(requestObject);
        
        if (!validateResult.IsValid)
        {
            return Results.ValidationProblem(validateResult.ToDictionary());
        }

        return await next(context);
    }
}