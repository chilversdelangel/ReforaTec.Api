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

        // 1. Hostage / abandoned account check:
        // Reject if the email is already registered AND verified.
        // If an account exists but was NEVER verified (e.g., abandoned signup or typo by another person),
        // we allow overwriting its details with this new attempt instead of permanently locking the student out.
        var existingUser = await FindUserByEmailAsync(context, normalizedEmail, cancellationToken);
        if (existingUser is { IsEmailVerified: true })
        {
            return Errors.EmailAlreadyExists;
        }

        // 2. Resolve educational institution (tenant) from email domain (e.g., "@cdmadero.tecnm.mx")
        var emailDomain = ExtractEmailDomain(normalizedEmail);
        var tenant = await FindTenantByDomainAsync(context, emailDomain, cancellationToken);
        if (tenant is null)
        {
            return Errors.InstitutionalDomainNotFound;
        }

        // 3. Claim control number within the tenant:
        // Ensures the requested control number is not already held by a verified student.
        // If held by an unverified account, it is reclaimed and cleared to prevent duplicate key collisions.
        var claimResult = await ClaimControlNumberAsync(
            context,
            requestedTenantId: tenant.Id,
            requestedControlNumber: request.ControlNumber,
            existingUserId: existingUser?.Id,
            cancellationToken);

        if (claimResult.IsError)
        {
            return claimResult.Errors;
        }

        // 4. Upsert user: reuse the unverified entity if one existed, or instantiate a new one
        var isNewUser = existingUser is null;
        var user = existingUser ?? new User();

        ApplyRegistration(user, request, tenant);

        if (isNewUser)
        {
            context.Users.Add(user);
        }

        await context.SaveChangesAsync(cancellationToken);

        // 5. Project to immutable response DTO
        return MapToResponse(user);
    }

    private static Task<User?> FindUserByEmailAsync(
        AppDbContext context,
        string normalizedEmail,
        CancellationToken cancellationToken)
    {
        return context.Users
            .FirstOrDefaultAsync(user => user.Email == normalizedEmail, cancellationToken);
    }

    private static string ExtractEmailDomain(string email)
    {
        return email.Split('@').Last();
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

    /// <summary>
    /// Checks control number uniqueness per tenant and prevents account hostage situations.
    /// If another verified student owns this number, registration is rejected with conflict.
    /// If an unverified account holds it, we set its ControlNumber to null, freeing it up in PostgreSQL
    /// so the current student can claim it without violating the UNIQUE(tenant_id, control_number) constraint.
    /// </summary>
    private static async Task<ErrorOr<Success>> ClaimControlNumberAsync(
        AppDbContext context,
        int requestedTenantId,
        string requestedControlNumber,
        int? existingUserId,
        CancellationToken cancellationToken)
    {
        var normalizedControlNumber = NormalizeControlNumber(requestedControlNumber);

        var conflictingUser = await context.Users
            .Where(user => user.TenantId == requestedTenantId)
            .Where(user => user.ControlNumber == normalizedControlNumber)
            .Where(user => user.Id != existingUserId)
            .FirstOrDefaultAsync(cancellationToken);

        if (conflictingUser is null)
        {
            return Result.Success;
        }

        if (conflictingUser.IsEmailVerified)
        {
            return Errors.ControlNumberAlreadyExists;
        }

        // Release the control number from the unverified account so PostgreSQL unique constraint won't fail
        conflictingUser.ControlNumber = null;
        return Result.Success;
    }

    private static string NormalizeControlNumber(string controlNumber)
    {
        return controlNumber.ToSanitized().ToUpperInvariant();
    }

    private static void ApplyRegistration(User user, Request request, Tenant tenant)
    {
        user.TenantId = tenant.Id;
        user.Email = request.Email;
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.SecondLastName = request.SecondLastName;
        user.ControlNumber = request.ControlNumber;
        user.PhoneNumber = request.PhoneNumber;
        user.Gender = request.Gender;
        user.CurrentRole = UserRole.Student;
    }

    private static Response MapToResponse(User user)
    {
        return new Response(
            user.Id,
            user.TenantId,
            user.Email,
            user.FirstName,
            user.LastName,
            user.SecondLastName,
            user.DisplayName,
            user.ControlNumber,
            user.PhoneNumber,
            user.Gender,
            user.CurrentRole,
            user.CreatedAt);
    }
}

