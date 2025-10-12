using DataAccess.Contract;
using DataAccess.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DataAccess;

public static class ServiceInjection
{
    public static void ConfigureDataAccess(IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.Services.AddHttpContextAccessor();
        builder.AddNpgsqlDbContext<AppDbContext>(connectionName: "postgresdb");
        builder.Services.AddScoped<IProductDataAccess, ProductDataAccess>(); 
        builder.Services.AddPlaywrightBrowserFactory();
        builder.Services.AddScoped<IHtmlFetcher, PlaywrightHtmlFetcher>();
    }
}