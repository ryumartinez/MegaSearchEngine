using Engine.Contract;
using Engine.EcommerceSearchScrappers;
using Engine.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Engine;

public static class ServiceInjection
{
    public static void ConfigureServices(IServiceCollection services)
    { 
        services.AddPlaywrightBrowserFactory();
        services.AddKeyedScoped<IEcommerceSearchScrapperEngine, PuntoFarmaSearchSearchScrapper>("puntofarma");
        services.AddKeyedScoped<IEcommerceSearchScrapperEngine, FarmaTotalSearchScrapperEngine>("farmaTotal");
        services.AddKeyedScoped<IEcommerceSearchScrapperEngine, BiggieSearchScrapperEngine>("biggie");
    }
}