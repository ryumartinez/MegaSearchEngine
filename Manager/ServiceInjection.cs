using Manager.Contract;
using Microsoft.Extensions.DependencyInjection;

namespace Manager;

public static class ServiceInjection
{
    public static void ConfigureServices(IServiceCollection services)
    { 
        services.AddScoped<ISearchResultItemManager, SearchResultItemManager>();
    }
}