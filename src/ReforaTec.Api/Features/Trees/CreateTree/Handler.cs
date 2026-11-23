using ErrorOr;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Common.Helpers;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities;
using ReforaTec.Api.Entities.Common;
using ReforaTec.Api.Entities.Enums;
using ReforaTec.Api.Infrastructure.Security.CurrentUser;

namespace ReforaTec.Api.Features.Trees.CreateTree;

internal static class Handler
{
    public static async Task<ErrorOr<Response>> Handle(
        UserIdentity loggedUser,
        Request request,
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        if (loggedUser.Role is not (UserRole.Coordinator or UserRole.SystemAdmin))
        {
            return Errors.Forbidden;
        }

        var normalizedFolio = request.Folio.ToNormalized();
        if (await FolioExistsAsync(context, loggedUser.TenantId, normalizedFolio, cancellationToken))
        {
            return Errors.FolioAlreadyExists(request.Folio.ToSanitized());
        }

        if (!await SpeciesExistsAsync(context, request.SpeciesId, cancellationToken))
        {
            return Errors.SpeciesNotFound(request.SpeciesId);
        }

        if (!await VirtueExistsAsync(context, request.VirtueId, cancellationToken))
        {
            return Errors.VirtueNotFound(request.VirtueId);
        }

        var tree = new Tree
        {
            TenantId = loggedUser.TenantId,
            Folio = request.Folio,
            SpeciesId = request.SpeciesId,
            VirtueId = request.VirtueId,
            PlantingDate = request.PlantingDate,
            Location = request.Location is not null
                ? new Location
                {
                    Latitude = request.Location.Latitude,
                    Longitude = request.Location.Longitude
                }
                : null
        };

        context.Trees.Add(tree);
        await context.SaveChangesAsync(cancellationToken);

        return MapToResponse(tree);
    }

    private static Task<bool> FolioExistsAsync(
        AppDbContext context,
        int tenantId,
        string normalizedFolio,
        CancellationToken cancellationToken) =>
        context.Trees
            .AsNoTracking()
            .Where(tree => tree.TenantId == tenantId)
            .Where(tree => tree.NormalizedFolio == normalizedFolio)
            .AnyAsync(cancellationToken);

    private static Task<bool> SpeciesExistsAsync(
        AppDbContext context,
        int speciesId,
        CancellationToken cancellationToken) =>
        context.Species
            .AsNoTracking()
            .AnyAsync(species => species.Id == speciesId, cancellationToken);

    private static Task<bool> VirtueExistsAsync(
        AppDbContext context,
        int virtueId,
        CancellationToken cancellationToken) =>
        context.Virtues
            .AsNoTracking()
            .AnyAsync(virtue => virtue.Id == virtueId, cancellationToken);

    private static Response MapToResponse(Tree tree) =>
        new(
            Id: tree.Id,
            Folio: tree.Folio,
            SpeciesId: tree.SpeciesId,
            VirtueId: tree.VirtueId,
            PlantingDate: tree.PlantingDate,
            Location: tree.Location is not null
                ? new TreeLocationDto(tree.Location.Latitude, tree.Location.Longitude)
                : null);
}
