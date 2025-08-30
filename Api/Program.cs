using Api.Endpoints;
using Microsoft.Playwright;
using Scalar.AspNetCore;
using Utils;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddOpenApi();
Manager.ServiceInjection.ConfigureServices(builder.Services);
DataAccess.ServiceInjection.ConfigureServices(builder.Services);

// --- Playwright Service Registration ---
// Register the IBrowser connection as a singleton. It will be created once
// when first requested and reused for subsequent requests.
builder.Services.AddSingleton(async sp =>
{
    // Get the fully built configuration
    var config = sp.GetRequiredService<IConfiguration>();
    var cdpEndpoint = config.GetConnectionString("cdp");

    if (string.IsNullOrEmpty(cdpEndpoint))
    {
        // This will provide a clear error if the connection string isn't found
        throw new InvalidOperationException(
            "Playwright CDP endpoint is not configured. " +
            "Ensure the 'cdp' connection string is set by the AppHost.");
    }

    Console.WriteLine($"Connecting to browser at: {cdpEndpoint}");

    var playwright = await Playwright.CreateAsync();
    return await playwright.Chromium.ConnectOverCDPAsync(cdpEndpoint);
});
// --- End of Playwright Registration ---


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

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}