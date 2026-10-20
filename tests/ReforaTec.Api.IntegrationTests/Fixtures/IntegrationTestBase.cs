using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities;
using ReforaTec.Api.Infrastructure.Security.Jwt;

namespace ReforaTec.Api.IntegrationTests.Fixtures;

[Collection(IntegrationTestCollection.Name)]
public abstract partial class IntegrationTestBase(ReforaTecWebApplicationFactory factory) : IAsyncLifetime
{
    protected ReforaTecWebApplicationFactory Factory { get; } = factory;
    protected HttpClient Client { get; } = factory.CreateClient();

    public virtual async Task InitializeAsync()
    {
        await Factory.ResetDatabaseAsync();
    }

    public virtual Task DisposeAsync() => Task.CompletedTask;

    protected void Authenticate(User user, string audience = Audience.StudentMobileApp)
    {
        using var scope = Factory.Services.CreateScope();
        var jwtService = scope.ServiceProvider.GetRequiredService<IJwtTokenService>();

        var token = jwtService.GenerateAccessToken(new TokenGenerationRequest(
            user.Id,
            user.TenantId,
            user.CurrentRole,
            audience));

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    protected async Task<T> QueryDbAsync<T>(Func<AppDbContext, Task<T>> query)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        return await query(context);
    }
}