using Mapster;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Database;

namespace ReforaTec.Api.Features.Values.GetValueById;

public static class GetValueById
{
    public record Response(
        int Id,
        string ValueName,
        DateTime CreatedAt,
        DateTime ModifiedAt
    );

    public static async Task<IResult> Handle(int id, AppDbContext context)
    {
        var value = await context.Values
            .AsNoTracking()
            .SingleOrDefaultAsync(t => t.Id == id);

        if (value is null)
        {
            return Results.Problem(
                statusCode: 404,
                title: "Value not found",
                detail: $"The value with ID {id} does not exist."
            );
        }

        return Results.Ok(value.Adapt<Response>());
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/values/{id:int}", Handle)
            .WithName("GetValueById");
    }
}