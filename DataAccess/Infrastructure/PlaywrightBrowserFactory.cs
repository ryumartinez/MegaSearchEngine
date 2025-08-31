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
                var playwright = await Playwright.CreateAsync();
                return await playwright.Chromium.ConnectOverCDPAsync(playwrightOptions.CdpEndpoint);
            }

            return CreateBrowserAsync();
        });

        // 3. Register our new factory as a singleton
        services.AddSingleton<IBrowserFactory, BrowserFactory>();

        return services;
    }
}