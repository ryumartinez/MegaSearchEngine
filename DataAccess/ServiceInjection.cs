using DataAccess.Contract.SearchResultItem;
using Microsoft.Extensions.DependencyInjection;

namespace DataAccess;

public static class ServiceInjection
{
    public static void ConfigureServices(IServiceCollection services)
    { 
        services.AddKeyedScoped<ISearchResultItemDataAccess, GoogleSearchResultItemDataAccess>("google");
        services.AddKeyedScoped<ISearchResultItemDataAccess, BingSearchResultItemDataAccess>("bing");
        services.AddKeyedScoped<ISearchResultItemDataAccess, DuckDuckGoSearchResultItemDataAccess>("duckduckgo");
    }
}