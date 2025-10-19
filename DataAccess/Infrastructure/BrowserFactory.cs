using Microsoft.Playwright;
using System.Diagnostics;
using OpenTelemetry.Trace; // Add this using directive

namespace DataAccess.Infrastructure;

public class BrowserFactory : IBrowserFactory
{
    private readonly Task<IBrowser> _browserTask;

    public BrowserFactory(Task<IBrowser> browserTask)
    {
        _browserTask = browserTask ?? throw new ArgumentNullException(nameof(browserTask));
    }

    public async Task<IPage> CreatePageAsync()
    {
        // Start a new activity for creating a page.
        // This will appear as a child span in your traces.
        using var activity = Tracing.Source.StartActivity("CreateBrowserPage");

        try
        {
            var browser = await _browserTask.ConfigureAwait(false);
            var page = await browser.NewPageAsync().ConfigureAwait(false);
            
            activity?.SetStatus(ActivityStatusCode.Ok);
            return page;
        }
        catch (Exception ex)
        {
            // If something goes wrong, record the exception on the activity
            activity?.RecordException(ex);
            activity?.SetStatus(ActivityStatusCode.Error, "Failed to create browser page");
            throw;
        }
    }
}