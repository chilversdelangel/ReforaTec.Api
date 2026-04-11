using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Database;
using ReforaTec.Api.Features.Trees.CreateTree;
using ReforaTec.Api.Features.Trees.GetTreeById;
using ReforaTec.Api.Features.Trees.GetTrees;
using ReforaTec.Api.Features.Values.CreateValue;
using ReforaTec.Api.Features.Values.GetValueById;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Services
builder.Services.AddOpenApi();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

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

app.Run();