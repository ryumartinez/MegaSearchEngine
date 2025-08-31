using Api.Endpoints;
using Microsoft.Playwright;
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

// --- New Scraping Endpoint ---
app.MapGet("/scrape", async (IBrowser browser) =>
{
    // A new page is created for each request to ensure isolation.
    await using var context = await browser.NewContextAsync();
    var page = await context.NewPageAsync();

    Console.WriteLine("Navigating to https://www.microsoft.com...");
    await page.GotoAsync("https://www.microsoft.com");

    var pageTitle = await page.TitleAsync();
    Console.WriteLine($"Page title: {pageTitle}");

    var content = await page.ContentAsync();
    Console.WriteLine($"Scraped {content.Length} characters of HTML.");

    // Return a JSON response to the caller
    return Results.Ok(new { title = pageTitle, contentLength = content.Length });
});
// --- End of Scraping Endpoint ---

app.Run();