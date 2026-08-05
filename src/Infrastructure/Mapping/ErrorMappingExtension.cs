using ErrorOr;

namespace ReforaTec.Api.Infrastructure.Mapping;

public static class ErrorMappingExtensions
{
    public static IResult ToProblem(this List<Error> errors)
    {
        if (errors.Count == 0) return Results.Problem();

        var firstError = errors[0];
        var errorResponse = new { code = firstError.Code, detail = firstError.Description };

        return firstError.Type switch
        {
            ErrorType.NotFound => Results.NotFound(errorResponse),

            ErrorType.Validation => Results.BadRequest(errorResponse),

            ErrorType.Conflict => Results.Conflict(errorResponse),

            // Uses Results.Json with 401 instead of Results.Unauthorized()
            // to preserve the custom JSON payload { code, detail } for client UIs.
            ErrorType.Unauthorized => Results.Json(
                errorResponse, 
                statusCode: StatusCodes.Status401Unauthorized),

            _ => Results.Problem(statusCode: 500, title: "An unexpected error occurred.")
        };
    }
}