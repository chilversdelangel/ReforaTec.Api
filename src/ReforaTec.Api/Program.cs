using FluentValidation;
using ReforaTec.Api.Database;
using ReforaTec.Api.Infrastructure.Endpoints;
using ReforaTec.Api.Infrastructure.Localization;
using ReforaTec.Api.Infrastructure.Middleware;
using ReforaTec.Api.Infrastructure.OpenApi;
using ReforaTec.Api.Infrastructure.Security.Jwt;
using ReforaTec.Api.Infrastructure.Security.Otp;
using ReforaTec.Api.Infrastructure.Storage;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Explicit Services Inventory
builder.Services.AddPostgresDbContext(builder.Configuration);
builder.Services.AddJwtAuthentication<JwtTokenService>(builder.Configuration);
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddFileStorage();
builder.Services.AddCustomOpenApi();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
    app.MapScalarApiReference().AllowAnonymous();

    await app.SeedDatabaseAsync();
}

app.UseHttpsRedirection();
app.UseDefaultRequestLocalization();
app.UseAuthentication();
app.UseAuthorization();

// Endpoints
var apiV1 = app.MapGroup("/api/v1")
    .WithTags("V1 Endpoints");

apiV1.MapEndpoints();

app.Run();