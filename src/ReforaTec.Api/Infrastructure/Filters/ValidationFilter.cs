using FluentValidation;

namespace ReforaTec.Api.Infrastructure.Filters;

public class ValidationFilter<T> : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next
    )
    {
        var requestObject = context.Arguments.OfType<T>().FirstOrDefault();
        if (requestObject == null) return await next(context);

        var validator = context.HttpContext.RequestServices.GetService<IValidator<T>>();
        if (validator == null) return await next(context);

        var validateResult = await validator.ValidateAsync(requestObject);

        if (!validateResult.IsValid)
        {
            return Results.ValidationProblem(validateResult.ToDictionary());
        }

        return await next(context);
    }
}