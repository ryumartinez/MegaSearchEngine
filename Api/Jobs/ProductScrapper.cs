using Manager.Contract;
using TickerQ.Utilities.Base;

namespace Api.Jobs;

internal sealed partial class ProductScraper
{
    private readonly ISearchResultItemManager _searchManager;
    private readonly ILogger<ProductScraper> _logger;
    
    public ProductScraper(
        ISearchResultItemManager searchManager, 
        ILogger<ProductScraper> logger)
    {
        _searchManager = searchManager;
        _logger = logger;
    }

    [TickerFunction("DailySearchBatch", cronExpression: "0 0 0 * * *")]
    public async Task ExecuteDailySearch(
        TickerFunctionContext context, 
        CancellationToken ct)
    {
        LogStartingDailyProductScrapingBatch();

        string[] dataToSend = ["first item", "second item", "third item"];

        try 
        {
            await _searchManager.SearchAndSaveManyAsync(dataToSend, ct).ConfigureAwait(true);
            LogSuccessfullyProcessedCountItems(dataToSend.Length);
        }
        catch (Exception ex)
        {
            LogFailedToExecuteDailySearchBatch(ex.Message);
            throw;
        }
    }

    [LoggerMessage(LogLevel.Information, "Starting daily product scraping batch.")]
    partial void LogStartingDailyProductScrapingBatch();

    [LoggerMessage(LogLevel.Information, "Successfully processed {count} items.")]
    partial void LogSuccessfullyProcessedCountItems(int count);

    [LoggerMessage(LogLevel.Error, "Failed to execute daily search batch. {message}")]
    partial void LogFailedToExecuteDailySearchBatch(string message);
}