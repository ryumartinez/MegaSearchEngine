using Api.Endpoints;
using DataAccess.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Utils;
using TickerQ.Dashboard.DependencyInjection;
using TickerQ.DependencyInjection;
using TickerQ.EntityFrameworkCore.Customizer;
using TickerQ.EntityFrameworkCore.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();

builder.Services.AddTickerQ(options =>
{
    options.AddOperationalStore(ef => 
    {
        ef.UseApplicationDbContext<AppDbContext>(ConfigurationType.UseModelCustomizer);
    });
    options.AddDashboard(dashboardOptions =>
    {
        dashboardOptions.SetBasePath("/admin/tickerq");
    });
});

builder.Services.AddOptions<PlaywrightOptions>()
    .Bind(builder.Configuration.GetSection(PlaywrightOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

DataAccess.ServiceInjection.ConfigureDataAccess(builder);
Engine.ServiceInjection.ConfigureServices(builder.Services);
Manager.ServiceInjection.ConfigureServices(builder.Services);

var app = builder.Build();
app.UseTickerQ();

using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

await dbContext.Database.MigrateAsync().ConfigureAwait(false);

app.MapOpenApi();
app.MapScalarApiReference();
app.UseHttpsRedirection();

app.MapSearchEndpoints();

await app.RunAsync().ConfigureAwait(false);