using System.Text.Json.Serialization;

namespace Scheduler.Services;

[JsonSerializable(typeof(string[]))]
internal sealed partial class DailyPostJsonSerializerContext : JsonSerializerContext
{
}

#pragma warning disable CA1812 
internal sealed class DailyPostService(IHttpClientFactory httpClientFactory) : BackgroundService
#pragma warning restore CA1812
{
    private const string TargetEndpointUrl = "https://api.example.com/receive-data";
    private readonly string[] _dataToSend = ["first item", "second item", "third item"];
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
         while (!stoppingToken.IsCancellationRequested)
         {
             try
             {
                 using var client = httpClientFactory.CreateClient();
                 var response = await client.PostAsJsonAsync(
                     TargetEndpointUrl, 
                     _dataToSend, 
                     DailyPostJsonSerializerContext.Default.StringArray,
                     stoppingToken).ConfigureAwait(false);
             }
             catch (OperationCanceledException)
             {
                 break;
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