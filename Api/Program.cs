using Api.Endpoints;
using Scalar.AspNetCore;
using Utils;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();
Manager.ServiceInjection.ConfigureServices(builder.Services);
DataAccess.ServiceInjection.ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapOpenApi();
app.MapScalarApiReference();
app.UseHttpsRedirection();
app.MapSearchEndpoints();

app.Run();