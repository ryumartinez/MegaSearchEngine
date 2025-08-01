using DataAccess.Contract.SearchResultItem;
using Microsoft.Extensions.DependencyInjection;

namespace DataAccess;

public static class ServiceInjection
{
    public static void ConfigureServices(IServiceCollection services)
    { 
        services.AddKeyedScoped<ISearchDataAccess, GoogleSearchDataAccess>("google");
        services.AddKeyedScoped<ISearchDataAccess, BingSearchDataAccess>("bing");
        services.AddKeyedScoped<ISearchDataAccess, DuckDuckGoSearchDataAccess>("duckduckgo");
    }
}