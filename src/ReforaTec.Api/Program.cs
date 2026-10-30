using FluentValidation;
using ReforaTec.Api.Database;
using ReforaTec.Api.Infrastructure.Endpoints;
using ReforaTec.Api.Infrastructure.Email;
using ReforaTec.Api.Infrastructure.Ingress;
using ReforaTec.Api.Infrastructure.Localization;
using ReforaTec.Api.Infrastructure.Middleware;
using ReforaTec.Api.Infrastructure.OpenApi;
using ReforaTec.Api.Infrastructure.Security.Authorization;
using ReforaTec.Api.Infrastructure.Security.Cors;
using ReforaTec.Api.Infrastructure.Security.CurrentUser;
using ReforaTec.Api.Infrastructure.Security.Jwt;
using ReforaTec.Api.Infrastructure.Security.Otp;
using ReforaTec.Api.Infrastructure.Security.RateLimiting;
using ReforaTec.Api.Infrastructure.Storage;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Explicit Services Inventory
builder.Services.AddPostgresDbContext(builder.Configuration);
builder.Services.AddLocalStorage(builder.Configuration);
builder.Services.AddCorsPolicy(builder.Configuration);
builder.Services.AddIngress(builder.Configuration);
builder.Services.AddCustomRateLimiting(builder.Configuration);
builder.Services.AddEmailServices(builder.Configuration);
builder.Services.AddJwtAuthentication<JwtTokenService>(builder.Configuration);
builder.Services.AddValidatorsFromAssemblyContaining<Program>(includeInternalTypes: true);
builder.Services.AddOtpService<OtpService>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddCurrentUserService();
builder.Services.AddProblemDetails();
builder.Services.AddMemoryCache();
builder.Services.AddAuthorizationPolicies();
builder.Services.AddCustomOpenApi();

var app = builder.Build();

app.UseExceptionHandler();
app.UseLocalStorage();

await app.MigrateDatabaseAsync();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
    app.MapScalarApiReference().AllowAnonymous();

    await app.SeedDatabaseAsync();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseRateLimiter();
app.UseDefaultRequestLocalization();
app.UseAuthentication();
app.UseAuthorization();

// Endpoints
var apiV1 = app.MapGroup("/v1");

apiV1.MapEndpoints();

app.Run();