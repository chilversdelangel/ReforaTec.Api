namespace ReforaTec.Api.Infrastructure.Localization;

internal static class LocalizationExtensions
{
    extension(WebApplication app)
    {
        internal WebApplication UseDefaultRequestLocalization()
        {
            var supportedCultures = new[] { "en" };
            app.UseRequestLocalization(options =>
            {
                options.SetDefaultCulture("en");
                options.AddSupportedCultures(supportedCultures);
                options.AddSupportedUICultures(supportedCultures);
            });

            return app;
        }
    }
}