using ErrorOr;

namespace ReforaTec.Api.Infrastructure.Mapping;

internal static class ErrorMappingExtensions
{
    public static IResult ToProblem(this List<Error> errors)
    {
        if (errors.Count == 0)
        {
            return Results.Problem();
        }

        var firstError = errors[0];
        
        var description = firstError.Description;
        var customCode = firstError.Code;

        var httpStatusCode = firstError.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };

        var extensions = new Dictionary<string, object?> 
        { 
            ["code"] = customCode 
        };

        return Results.Problem(
            statusCode: httpStatusCode,
            detail: description,
            extensions: extensions
        );
    }
}