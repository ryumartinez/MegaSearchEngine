using Api.Endpoints;
using DataAccess.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Utils;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();
builder.Services.AddOptions<PlaywrightOptions>()
    .Bind(builder.Configuration.GetSection(PlaywrightOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

DataAccess.ServiceInjection.ConfigureDataAccess(builder);
Engine.ServiceInjection.ConfigureServices(builder.Services);
Manager.ServiceInjection.ConfigureServices(builder.Services);

var app = builder.Build();

using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

// Apply any pending migrations
await dbContext.Database.MigrateAsync().ConfigureAwait(false);

// Configure the HTTP request pipeline.
app.MapOpenApi();
app.MapScalarApiReference();
app.UseHttpsRedirection();
app.MapSearchEndpoints();

await app.RunAsync().ConfigureAwait(false);