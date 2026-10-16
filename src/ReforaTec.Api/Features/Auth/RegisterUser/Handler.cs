using ErrorOr;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Common.Helpers;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities;
using ReforaTec.Api.Entities.Enums;

namespace ReforaTec.Api.Features.Auth.RegisterUser;

internal static class Handler
{
    public static async Task<ErrorOr<Response>> Handle(
        Request request,
        AppDbContext context,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.ToNormalized();

        var user = await context.Users
            .FirstOrDefaultAsync(user => user.Email == normalizedEmail, cancellationToken);

        if (user is not null && user.IsEmailVerified)
        {
            return Errors.EmailAlreadyExists;
        }

        // If the user exists but is NOT verified, we allow overwriting their data (Hostage mitigation)
        var emailDomain = normalizedEmail.Split('@').Last();

        var tenant = await FindTenantByDomainAsync(context, emailDomain, cancellationToken);

        if (tenant is null)
        {
            return Errors.InstitutionalDomainNotFound;
        }

        var controlNumber = NormalizeControlNumber(request.ControlNumber);
        var existingUserId = user?.Id;

        var controlNumberOwner = await context.Users.FirstOrDefaultAsync(
            candidate => 
                candidate.TenantId == tenant.Id && 
                candidate.ControlNumber == controlNumber &&
                candidate.Id != existingUserId,
            cancellationToken);

        if (controlNumberOwner?.IsEmailVerified is true)
        {
            return Errors.ControlNumberAlreadyExists;
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

        return new Response(
            userEntity.Id,
            userEntity.TenantId,
            userEntity.Email,
            userEntity.FirstName,
            userEntity.MiddleName,
            userEntity.LastName,
            userEntity.SecondLastName,
            userEntity.ControlNumber,
            userEntity.PhoneNumber,
            userEntity.Gender,
            userEntity.CurrentRole,
            userEntity.CreatedAt);
    }

    private static Task<Tenant?> FindTenantByDomainAsync(
        AppDbContext context,
        string emailDomain,
        CancellationToken cancellationToken)
    {
        return context.Tenants
            .FirstOrDefaultAsync(
                tenant => tenant.InstitutionalEmailDomain == emailDomain && !tenant.IsDeleted,
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
        userToSave.Gender = request.Gender;
        userToSave.CurrentRole = UserRole.Student; // Default role for open registrations
    }
}