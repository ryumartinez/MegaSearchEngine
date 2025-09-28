using Api.Endpoints;
using Engine.Infrastructure;
using Scalar.AspNetCore;
using Utils;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();
Manager.ServiceInjection.ConfigureServices(builder.Services);
builder.Services.AddOptions<PlaywrightOptions>()
    .Bind(builder.Configuration.GetSection(PlaywrightOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapOpenApi();
app.MapScalarApiReference();
app.UseHttpsRedirection();
app.MapSearchEndpoints();

app.Run();