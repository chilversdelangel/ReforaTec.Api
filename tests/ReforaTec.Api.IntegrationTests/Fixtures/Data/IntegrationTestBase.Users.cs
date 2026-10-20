using Microsoft.Extensions.DependencyInjection;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities;
using ReforaTec.Api.Entities.Enums;

// ReSharper disable once CheckNamespace
namespace ReforaTec.Api.IntegrationTests.Fixtures;

public abstract partial class IntegrationTestBase
{
    protected async Task<User> CreateUserAsync(
        int tenantId = 1,
        
        string? email = null,
        bool isEmailVerified = true,
        
        string? controlNumber = null,
        string givenName = "Juan",
        string familyName = "Pérez",
        
        Gender gender = Gender.Male,
        UserRole role = UserRole.Student,
        
        bool isDeleted = false)
    {
        using var scope = Factory.Services.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var now = DateTime.UtcNow;
        var user = new User
        {
            TenantId = tenantId,
            
            Email = email ?? $"user_{Guid.NewGuid().ToString("N")[..8]}@cdmadero.tecnm.mx",
            IsEmailVerified = isEmailVerified,
            
            ControlNumber = controlNumber ?? $"20{Random.Shared.Next(100000, 999999)}",
            GivenName = givenName,
            FamilyName = familyName,
            
            Gender = gender,
            CurrentRole = role,
            
            IsDeleted = isDeleted,
            DeletedAt = isDeleted ? now : null
        };

        user.Normalize();

        appDbContext.Users.Add(user);
        await appDbContext.SaveChangesAsync();

        return user;
    }
}