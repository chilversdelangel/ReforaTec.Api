using FluentValidation;
using Mapster;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities;
using ReforaTec.Api.Infrastructure.Filters;

namespace ReforaTec.Api.Features.Values.CreateValue;

public static class CreateValue
{
    public record Request(string ValueName);

    public record Response(
        int Id,
        string ValueName,
        DateTime CreatedAt,
        DateTime ModifiedAt
    );

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.ValueName)
                .NotEmpty()
                .WithMessage("ValueName is required")
                .Must(name => name.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
                .WithMessage("ValueName contains invalid characters")
                .Length(1, 25)
                .WithMessage("ValueName length must be between 1 and 25");
        }
    }

    public static async Task<IResult> Handle(Request request, AppDbContext context)
    {
        var newValue = request.Adapt<Value>();
        
        context.Values.Add(newValue);
        await context.SaveChangesAsync();
        
        var valueResponse = newValue.Adapt<Response>();

        return Results.CreatedAtRoute("GetValueById", new { id = valueResponse.Id }, valueResponse);
    }
    
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/values", Handle)
            .AddEndpointFilter<ValidationFilter<Request>>()
            .Produces<Response>(StatusCodes.Status201Created)
            .ProducesValidationProblem();
    }
}