using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Entities;
using ReforaTec.Api.Entities.Enums;

namespace ReforaTec.Api.Database.Seeding;

internal static partial class DbSeeder
{
    private static async Task SeedDevelopmentUsersAsync(AppDbContext context)
    {
        if (await context.Users.AnyAsync(user => user.Email == "student@cdmadero.tecnm.mx"))
        {
            return;
        }

        var tenant = await context.Tenants.FirstAsync(tenant => tenant.Acronym == "ITCM");

        var testUser = new User
        {
            TenantId = tenant.Id,
            CurrentRole = UserRole.Student,
            Email = "student@cdmadero.tecnm.mx",
            IsEmailVerified = true,
            ControlNumber = "21070001",
            GivenName = "Juan",
            FamilyName = "Pérez",
            SecondFamilyName = "Hernández",
            DisplayName = "Juancho",
            PhoneNumber = "+528331234567",
            Gender = Gender.Male
        };
        testUser.Normalize();

        var deletedUser = new User
        {
            TenantId = tenant.Id,
            CurrentRole = UserRole.Student,
            Email = "deleted_student@cdmadero.tecnm.mx",
            IsEmailVerified = false,
            ControlNumber = "21070002",
            GivenName = "Carlos",
            FamilyName = "Gómez",
            Gender = Gender.Male,
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow
        };
        deletedUser.Normalize();

        var adminUser = new User
        {
            TenantId = tenant.Id,
            CurrentRole = UserRole.SystemAdmin,
            Email = "admin@cdmadero.tecnm.mx",
            IsEmailVerified = true,
            GivenName = "Administrador",
            FamilyName = "Sistema",
            DisplayName = "Admin",
            Gender = Gender.Other
        };
        adminUser.Normalize();

        context.Users.AddRange(testUser, deletedUser, adminUser);
        await context.SaveChangesAsync();
    }
}
