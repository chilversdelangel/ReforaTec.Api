namespace ReforaTec.Api.Infrastructure.OpenApi;

internal static class OpenApiExtensions
{
    extension(IServiceCollection services)
    {
        internal IServiceCollection AddCustomOpenApi()
        {
            services.AddOpenApi(options =>
            {
                options.CreateSchemaReferenceId = typeInfo => typeInfo.Type.FullName?
                    .Replace("ReforaTec.Api.Features.", "")
                    .Replace("+", ".");
            });

            return services;
        }
    }
}