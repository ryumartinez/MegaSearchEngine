using DataAccess.Contract.SearchResultItem;
using Microsoft.Extensions.DependencyInjection;

namespace DataAccess;

public static class ServiceInjection
{
    public static void ConfigureServices(IServiceCollection services)
    { 
        services.AddHttpClient();
        services.AddKeyedScoped<ISearchDataAccess, GoogleSearchDataAccess>("google");
        services.AddKeyedScoped<ISearchDataAccess, BingSearchDataAccess>("bing");
        services.AddKeyedScoped<ISearchDataAccess, DuckDuckGoSearchDataAccess>("duckduckgo");
        services.AddKeyedScoped<ISearchDataAccess, PuntoFarmaSearchDataAccess>("puntofarma");
        services.AddKeyedScoped<ISearchDataAccess, FarmacenterSearchDataAccess>("farmacenter");
        services.AddKeyedScoped<ISearchDataAccess, FarmaTotalSearchDataAccess>("farmatotal");
        services.AddKeyedScoped<ISearchDataAccess, FarmaOlivaSearchDataAccess>("farmaoliva");
    }
}