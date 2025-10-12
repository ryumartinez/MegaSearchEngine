using System.Net.Http;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Hosting;

namespace Scheduler.Services;

[JsonSerializable(typeof(string[]))]
internal sealed partial class DailyPostJsonSerializerContext : JsonSerializerContext
{
}

#pragma warning disable CA1812
internal sealed class DailyPostService(IHttpClientFactory httpClientFactory) : BackgroundService
#pragma warning restore CA1812
{
    // 1. The hardcoded URL is removed.
    private readonly string[] _dataToSend = ["first item", "second item", "third item"];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
         while (!stoppingToken.IsCancellationRequested)
         {
             try
             {
                 // 2. Use the named "gateway" client configured in your Program.cs
                 using var client = httpClientFactory.CreateClient("api");

                 // 3. Use the relative path that the gateway expects to route to the API
                 var response = await client.PostAsJsonAsync(
                     "/search/run/batch",
                     _dataToSend,
                     DailyPostJsonSerializerContext.Default.StringArray,
                     stoppingToken).ConfigureAwait(false);

                 // 4. (Recommended) Check if the API call was successful (e.g., returned a 200 OK)
                 response.EnsureSuccessStatusCode();
             }
             catch (OperationCanceledException)
             {
                 // The application is shutting down, so we exit the loop.
                 break;
             }
             catch (HttpRequestException ex)
             {
                 // 5. (Recommended) Add handling for network or API errors
                 // In a real app, you would use ILogger here.
                 Console.WriteLine($"Error calling the gateway: {ex.Message}");
             }

             try
             {
                 await Task.Delay(TimeSpan.FromDays(1), stoppingToken).ConfigureAwait(false);
             }
             catch (OperationCanceledException)
             {
                 break;
             }
         }
    }
}