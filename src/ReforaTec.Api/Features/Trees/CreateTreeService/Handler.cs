using ErrorOr;
using Mapster;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities;
using ReforaTec.Api.Infrastructure.Security.CurrentUser;

namespace ReforaTec.Api.Features.Trees.CreateTreeService;

internal static class Handler
{
    public static async Task<ErrorOr<Response>> Handle(
        int treeId,
        UserIdentity user,
        Request request,
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        var treeExists = await context.Trees
            .Where(tree => tree.Id == treeId)
            .Where(tree => tree.TenantId == user.TenantId)
            .AnyAsync(cancellationToken);

        if (!treeExists)
        {
            return Errors.TreeNotFound;
        }

        var assignment = await context.UserCaresForTrees
            .Where(assignment => assignment.UserId == user.UserId)
            .Where(assignment => assignment.TreeId == treeId)
            .Where(assignment => assignment.EndDate == null)
            .SingleOrDefaultAsync(cancellationToken);

        if (assignment is null)
        {
            return Errors.UserNotAssignedToTree;
        }

        var serviceTypeExists = await context.ServiceTypes
            .Where(serviceType => serviceType.Id == request.ServiceTypeId)
            .AnyAsync(cancellationToken);

        if (!serviceTypeExists)
        {
            return Errors.ServiceTypeNotFound;
        }

        var service = new Service
        {
            TenantId = user.TenantId,
            StudentId = user.UserId,
            TreeId = treeId,
            ServiceTypeId = request.ServiceTypeId,
            CampaignId = assignment.CampaignId,
            DeviceCapturedAt = request.DeviceCapturedAt.UtcDateTime,
            Comment = request.Comment
        };

        context.Services.Add(service);
        await context.SaveChangesAsync(cancellationToken);

        return service.Adapt<Response>();
    }
}