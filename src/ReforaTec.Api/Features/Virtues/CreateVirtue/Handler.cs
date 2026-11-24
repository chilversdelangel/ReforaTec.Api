using ErrorOr;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Common.Helpers;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities;

namespace ReforaTec.Api.Features.Virtues.CreateVirtue;

internal static class Handler
{
    public static async Task<ErrorOr<Response>> Handle(
        Request request,
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        var normalizedName = request.Name.ToNormalized();

        if (await NameExistsAsync(context, normalizedName, cancellationToken))
        {
            return Errors.DuplicateName(request.Name.ToSanitized());
        }

        var virtue = new Virtue
        {
            Name = request.Name
        };

        context.Virtues.Add(virtue);
        await context.SaveChangesAsync(cancellationToken);

        return new Response(virtue.Id, virtue.Name);
    }

    private static Task<bool> NameExistsAsync(
        AppDbContext context,
        string normalizedName,
        CancellationToken cancellationToken)
    {
        return context.Virtues
            .AsNoTracking()
            .Where(virtue => virtue.NormalizedName == normalizedName)
            .AnyAsync(cancellationToken);
    }
}
