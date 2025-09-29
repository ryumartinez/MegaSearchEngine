using Api.Endpoints;
using DataAccess.Infrastructure;
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

// Configure the HTTP request pipeline.
app.MapOpenApi();
app.MapScalarApiReference();
app.UseHttpsRedirection();
app.MapSearchEndpoints();

app.Run();