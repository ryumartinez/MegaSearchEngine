using Microsoft.Playwright;

namespace DataAccess.Infrastructure;

public class BrowserFactory : IBrowserFactory
{
    private readonly Task<IBrowser> _browserTask;

    // We inject the Task<IBrowser> singleton we registered earlier.
    public BrowserFactory(Task<IBrowser> browserTask)
    {
        _browserTask = browserTask ?? throw new ArgumentNullException(nameof(browserTask));
    }

    public async Task<IPage> CreatePageAsync()
    {
        // Await the browser initialization task.
        // This will only block on the very first call; subsequent calls will be instant.
        var browser = await _browserTask.ConfigureAwait(false);
        
        // Return a new page from the singleton browser instance.
        return await browser.NewPageAsync().ConfigureAwait(false);
    }
}