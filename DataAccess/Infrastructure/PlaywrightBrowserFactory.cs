using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;

namespace DataAccess.Infrastructure;

public static class PlaywrightBrowserFactory
{
    public static IServiceCollection AddPlaywrightBrowserFactory(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Register the Options
        services.AddOptions<PlaywrightOptions>()
            .Bind(configuration.GetSection(PlaywrightOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // 2. Register the IBrowser initialization task as a singleton
        services.AddSingleton<Task<IBrowser>>(sp =>
        {
            var playwrightOptions = sp.GetRequiredService<IOptions<PlaywrightOptions>>().Value;
            Console.WriteLine($"Connecting to browser at: {playwrightOptions.CdpEndpoint}");
            
            async Task<IBrowser> CreateBrowserAsync()
            {
                // --- THIS IS THE FIX ---
                // The 'playwright run-server' command provides a Playwright-specific WebSocket endpoint,
                // not a standard CDP/JSON endpoint. We must use ConnectAsync() and a 'ws://' scheme.
                
                var httpEndpoint = playwrightOptions.CdpEndpoint;
                var wsEndpoint = $"ws://{new Uri(httpEndpoint).Authority}";

                Console.WriteLine($"Attempting to connect to Playwright server at: {wsEndpoint}");

                var playwright = await Playwright.CreateAsync();
                
                // Use ConnectAsync for Playwright servers, not ConnectOverCDPAsync
                return await playwright.Chromium.ConnectAsync(wsEndpoint);
            }

            return CreateBrowserAsync();
        });

        // 3. Register our new factory as a singleton
        services.AddSingleton<IBrowserFactory, BrowserFactory>();

        return services;
    }
}