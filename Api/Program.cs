using Api.Endpoints;
using DataAccess.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using TickerQ.Dashboard.DependencyInjection;
using TickerQ.DependencyInjection;
using TickerQ.EntityFrameworkCore.DbContextFactory;
using TickerQ.EntityFrameworkCore.DependencyInjection;
using Utils;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();
builder.Services.AddOptions<PlaywrightOptions>()
    .Bind(builder.Configuration.GetSection(PlaywrightOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddTickerQ(options =>
{
    options.ConfigureScheduler(schedulerOptions =>
    {
        schedulerOptions.MaxConcurrency = 10;
        schedulerOptions.NodeIdentifier = "scheduler-node-1";
    });

    // Use Postgres instead of SqlServer
    options.AddOperationalStore(efOptions =>
    {
        efOptions.UseTickerQDbContext<TickerQDbContext>(optionsBuilder =>
        {
            // Aspire provides the connection string via "tickerq-db" name
            optionsBuilder.UseNpgsql(builder.Configuration.GetConnectionString("tickerq-db"));
        });
    });

    // Optional: Add Dashboard (since you have a Proxy project, this is useful)
    options.AddDashboard(dashboardOptions =>
    {
        dashboardOptions.SetBasePath("/admin/tickerq");
    });
});

DataAccess.ServiceInjection.ConfigureDataAccess(builder);
Engine.ServiceInjection.ConfigureServices(builder.Services);
Manager.ServiceInjection.ConfigureServices(builder.Services);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    // 1. Migrate your main app database (Keep this as MigrateAsync)
    var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await appDbContext.Database.MigrateAsync().ConfigureAwait(false);

    // 2. FORCE create the TickerQ schema without using migrations
    var tickerQDbContext = scope.ServiceProvider.GetRequiredService<TickerQDbContext>();
    await tickerQDbContext.Database.EnsureCreatedAsync().ConfigureAwait(false); 
}

// Configure the HTTP request pipeline.
app.MapOpenApi();
app.MapScalarApiReference();
app.UseHttpsRedirection();
app.UseTickerQ();
app.MapSearchEndpoints();

await app.RunAsync().ConfigureAwait(false);