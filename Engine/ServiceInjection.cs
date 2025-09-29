using Engine.Contract;
using Engine.EcommerceSearchScrappers;
using Microsoft.Extensions.DependencyInjection;

namespace Engine;

public static class ServiceInjection
{
    public static void ConfigureServices(IServiceCollection services)
    { 
       
        services.AddKeyedScoped<IEcommerceParserEngine, PuntoFarmaParserEngine>("puntoFarma");
        services.AddKeyedScoped<IEcommerceParserEngine, FarmaTotalParserEngine>("farmaTotal");
        services.AddKeyedScoped<IEcommerceParserEngine, BiggieParserEngine>("biggie");
    }
}