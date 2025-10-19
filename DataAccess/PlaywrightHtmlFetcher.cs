using DataAccess.Contract;
using DataAccess.Infrastructure;
using Microsoft.Playwright;
using System.Diagnostics;
using OpenTelemetry.Trace; // Add this using directive

namespace DataAccess;

public sealed class PlaywrightHtmlFetcher : IHtmlFetcher
{
    private readonly IBrowserFactory _browserFactory;

    public PlaywrightHtmlFetcher(IBrowserFactory browserFactory)
        => _browserFactory = browserFactory;

    public async Task<string> GetContentAsync(Uri url, CancellationToken ct = default)
    {
        return await GetContentAsync(url, readySelector: null, readyTimeoutMs: 0, ct).ConfigureAwait(false);
    }

    public async Task<string> GetContentAsync(Uri url, string? readySelector, int readyTimeoutMs = 15000, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(url);

        // This is the main parent activity for the entire operation.
        using var activity = Tracing.Source.StartActivity("FetchPageContent", ActivityKind.Client);
        
        // Add useful tags (attributes) for better observability.
        activity?.SetTag("url.full", url.ToString());
        activity?.SetTag("url.host", url.Host);
        activity?.SetTag("playwright.ready_selector", readySelector);
        activity?.SetTag("playwright.timeout", readyTimeoutMs);

        try
        {
            var page = await _browserFactory.CreatePageAsync().ConfigureAwait(false);
            page.SetDefaultTimeout(20000);

            // Add an event to mark a specific point in time within the activity.
            activity?.AddEvent(new ActivityEvent("Navigating to URL"));
            await page.GotoAsync(url.ToString(), new() { WaitUntil = WaitUntilState.DOMContentLoaded }).ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(readySelector))
            {
                try
                {
                    activity?.AddEvent(new ActivityEvent("Waiting for ready selector"));
                    await page.Locator(readySelector).First.WaitForAsync(new()
                    {
                        State = WaitForSelectorState.Visible,
                        Timeout = readyTimeoutMs
                    }).ConfigureAwait(false);
                }
                catch (TimeoutException ex)
                {
                    activity?.AddEvent(new ActivityEvent("Selector wait timed out"));
                    activity?.RecordException(ex); // Record it as a non-fatal exception
                }
            }

            var content = await SafeContentAsync(page, activity).ConfigureAwait(false);
            activity?.SetTag("http.response_content_length", content.Length);
            activity?.SetTag("http.response_content", content);
            activity?.SetStatus(ActivityStatusCode.Ok);
            return content;
        }
        catch (Exception ex)
        {
            // This catches fatal exceptions and marks the activity as failed.
            activity?.RecordException(ex);
            activity?.SetStatus(ActivityStatusCode.Error, "Failed to fetch page content");
            throw;
        }
    }

    // Pass the activity down to record retry events.
    private static async Task<string> SafeContentAsync(IPage page, Activity? activity)
    {
        for (int i = 0; i < 5; i++)
        {
            try { return await page.ContentAsync().ConfigureAwait(false); }
            catch (PlaywrightException ex)
                when (ex.Message.Contains("Execution context was destroyed", StringComparison.OrdinalIgnoreCase))
            {
                activity?.AddEvent(new ActivityEvent($"Content fetch retry attempt {i + 1}"));
                await Task.Delay(200).ConfigureAwait(false);
            }
        }
        return await page.ContentAsync().ConfigureAwait(false);
    }
}