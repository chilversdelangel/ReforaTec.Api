using ErrorOr;

namespace ReforaTec.Api.Infrastructure.Mapping;

public static class ErrorMappingExtensions
{
    public static IResult ToProblem(this List<Error> errors)
    {
        if (errors.Count == 0) Results.Problem();

        var firstError = errors[0];

        return firstError.Type switch
        {
            ErrorType.NotFound => Results.NotFound(new
                { code = firstError.Code, detail = firstError.Description }),

            ErrorType.Validation => Results.BadRequest(new
                { code = firstError.Code, detail = firstError.Description }),

            ErrorType.Conflict => Results.Conflict(new
                { code = firstError.Code, detail = firstError.Description }),

            _ => Results.Problem(statusCode: 500, title: "An unexpected error occurred.")
        };
    }
}