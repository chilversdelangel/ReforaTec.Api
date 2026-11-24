using ErrorOr;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Common.Helpers;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities;

namespace ReforaTec.Api.Features.CareTypes.CreateCareType;

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

        var careType = new CareType
        {
            Name = request.Name,
            IconUrl = request.IconUrl.ToSanitized(),
            IsActive = request.IsActive
        };

        context.CareTypes.Add(careType);
        await context.SaveChangesAsync(cancellationToken);

        return new Response(
            careType.Id,
            careType.Name,
            careType.IconUrl,
            careType.IsActive);
    }

    private static Task<bool> NameExistsAsync(
        AppDbContext context,
        string normalizedName,
        CancellationToken cancellationToken)
    {
        return context.CareTypes
            .AsNoTracking()
            .Where(careType => careType.NormalizedName == normalizedName)
            .AnyAsync(cancellationToken);
    }
}
