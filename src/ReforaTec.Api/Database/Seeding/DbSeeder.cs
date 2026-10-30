namespace ReforaTec.Api.Database.Seeding;

/// <summary>
/// Coordinates database seeding across master catalogs and development mock data.
/// </summary>
internal static partial class DbSeeder
{
    public static async Task SeedCatalogsAsync(AppDbContext context)
    {
        await SeedTenantsAsync(context);
    }

    public static async Task SeedDevelopmentDataAsync(AppDbContext context)
    {
        await SeedSpeciesAsync(context);
        await SeedValuesAsync(context);
        await SeedServiceTypesAsync(context);
        await SeedDevelopmentUsersAsync(context);
        await SeedTreesAsync(context);
        await SeedCampaignsAsync(context);
        await SeedTreesToCampaignAsync(context);
        await SeedStudentAssignmentsAsync(context);
        await SeedMeasurementsAsync(context);
    }
}
