using ErrorOr;
using Mapster;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Common.Helpers;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities;
using ReforaTec.Api.Entities.Enums;

namespace ReforaTec.Api.Features.Auth.RegisterUser;

public static class Handler
{
    public static async Task<ErrorOr<Response>> Handle(
        Request request,
        AppDbContext context,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.ToNormalized();

        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (user is not null && user.IsEmailVerified)
        {
            return Error.Conflict(
                code: ErrorCodes.EmailAlreadyExists,
                description: "A user with this email already exists.");
        }

        // If the user exists but is NOT verified, we allow overwriting their data (Hostage mitigation)
        var emailDomain = normalizedEmail.Split('@').Last();

        var tenant = await FindTenantByDomainAsync(context, emailDomain, cancellationToken);

        if (tenant is null)
        {
            return Error.NotFound(
                code: ErrorCodes.InstitutionalDomainNotFound,
                description: "The provided email domain does not belong to any registered institution.");
        }

        var controlNumber = NormalizeControlNumber(request.ControlNumber);
        var existingUserId = user?.Id;

        var controlNumberOwner = await context.Users.FirstOrDefaultAsync(
            u => u.TenantId == tenant.Id &&
                 u.ControlNumber == controlNumber &&
                 u.Id != existingUserId,
            cancellationToken);

        if (controlNumberOwner?.IsEmailVerified == true)
        {
            return Error.Conflict(
                code: ErrorCodes.ControlNumberAlreadyExists,
                description: "A user with this control number already exists in the institution.");
        }
            
        // Release the hijacked control number from the unverified
        controlNumberOwner?.ControlNumber = null;

        var userEntity = user ?? new User();
        ApplyRegistration(userEntity, request, tenant);
        
        var isNewUser = user is null;

        if (isNewUser)
        {
            context.Users.Add(userEntity);
        }

        await context.SaveChangesAsync(cancellationToken);

        return userEntity.Adapt<Response>();
    }

    private static Task<Tenant?> FindTenantByDomainAsync(
        AppDbContext context,
        string emailDomain,
        CancellationToken cancellationToken)
    {
        return context.Tenants
            .FirstOrDefaultAsync(
                t => t.InstitutionalEmailDomain == emailDomain && !t.IsDeleted,
                cancellationToken);
    }

    private static string NormalizeControlNumber(string controlNumber)
    {
        return controlNumber.ToSanitized().ToUpperInvariant();
    }

    private static void ApplyRegistration(User userToSave, Request request, Tenant tenant)
    {
        userToSave.TenantId = tenant.Id;
        userToSave.Email = request.Email;
        userToSave.FirstName = request.FirstName;
        userToSave.MiddleName = request.MiddleName;
        userToSave.LastName = request.LastName;
        userToSave.SecondLastName = request.SecondLastName;
        userToSave.ControlNumber = request.ControlNumber;
        userToSave.PhoneNumber = request.PhoneNumber;
        userToSave.CurrentRole = UserRole.Student; // Default role for open registrations
    }
}