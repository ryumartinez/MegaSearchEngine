using Engine.Contract;
using Engine.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Engine;

public static class ServiceInjection
{
    public static void ConfigureServices(IServiceCollection services)
    { 
        services.AddPlaywrightBrowserFactory();
        services.AddKeyedScoped<IEcommerceSearchScrapperEngine, PuntoFarmaSearchSearchScrapper>("puntofarma");
    }
}