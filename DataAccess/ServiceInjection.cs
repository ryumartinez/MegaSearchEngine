using DataAccess.Infrastructure;
using Microsoft.Extensions.Hosting;

namespace DataAccess;

public static class ServiceInjection
{
    public static void ConfigureDataAccess(IHostApplicationBuilder builder)
    {
        builder.AddNpgsqlDbContext<AppDbContext>(connectionName: "postgresdb");
    }
}