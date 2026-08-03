using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Database;
using ReforaTec.Api.Features.Auth.RefreshSession;
using ReforaTec.Api.Features.Auth.RequestOtp;
using ReforaTec.Api.Features.Auth.RevokeToken;
using ReforaTec.Api.Features.Auth.VerifyOtp;
using ReforaTec.Api.Features.Campaigns.CreateCampaign;
using ReforaTec.Api.Features.Campaigns.GetCampaignById;
using ReforaTec.Api.Features.Species.CreateSpecies;
using ReforaTec.Api.Features.Species.GetSpeciesById;
using ReforaTec.Api.Features.Trees.CreateTree;
using ReforaTec.Api.Features.Trees.GetTreeById;
using ReforaTec.Api.Features.Trees.GetTrees;
using ReforaTec.Api.Features.Values.CreateValue;
using ReforaTec.Api.Features.Values.GetValueById;
using ReforaTec.Api.Infrastructure.Middleware;
using ReforaTec.Api.Infrastructure.Security.Jwt;
using ReforaTec.Api.Infrastructure.Security.Otp;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Services
builder.Services.AddOpenApi(options =>
{
    options.CreateSchemaReferenceId = typeInfo => typeInfo.Type.FullName?
        .Replace("ReforaTec.Api.Features.", "")
        .Replace("+", ".");
});
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString);
    options.UseSnakeCaseNamingConvention();
});

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IOtpService, OtpService>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();

    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbSeeder.SeedAsync(dbContext);
}

app.UseHttpsRedirection();

var supportedCultures = new[] { "en" };
app.UseRequestLocalization(options =>
{
    options.SetDefaultCulture("en");
    options.AddSupportedCultures(supportedCultures);
    options.AddSupportedUICultures(supportedCultures);
});

// Endpoints
var apiV1 = app.MapGroup("/api/v1")
    .WithTags("V1 Endpoints");

// Trees
GetTrees.MapEndpoint(apiV1);
GetTreeById.MapEndpoint(apiV1);
CreateTree.MapEndpoint(apiV1);

// Values
GetValueById.MapEndpoint(apiV1);
CreateValue.MapEndpoint(apiV1);

// Species
CreateSpecies.MapEndpoint(apiV1);
GetSpeciesById.MapEndpoint(apiV1);

// Campaign
GetCampaignById.MapEndpoint(apiV1);
CreateCampaign.MapEndpoint(apiV1);

// Auth
RequestOtp.MapEndpoint(apiV1);
VerifyOtp.MapEndpoint(apiV1);
RefreshSession.MapEndpoint(apiV1);
RevokeToken.MapEndpoint(apiV1);

app.Run();