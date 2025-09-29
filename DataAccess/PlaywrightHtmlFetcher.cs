using DataAccess.Contract;
using DataAccess.Infrastructure;
using Microsoft.Playwright;

namespace DataAccess;

public sealed class PlaywrightHtmlFetcher : IHtmlFetcher
{
    private readonly IBrowserFactory _browserFactory;

    public PlaywrightHtmlFetcher(IBrowserFactory browserFactory)
        => _browserFactory = browserFactory;

    public async Task<string> GetContentAsync(Uri url, CancellationToken ct = default)
    {
        // fallback to “no special wait”
        return await GetContentAsync(url, readySelector: null, readyTimeoutMs: 0, ct).ConfigureAwait(false);
    }

    public async Task<string> GetContentAsync(Uri url, string? readySelector, int readyTimeoutMs = 15000, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(url);
        var page = await _browserFactory.CreatePageAsync().ConfigureAwait(false);
        page.SetDefaultTimeout(20000);

        await page.GotoAsync(url.ToString(), new() { WaitUntil = WaitUntilState.DOMContentLoaded }).ConfigureAwait(false);

        // Key: wait like your old code did — for the site’s product selector — instead of poking at title/HTML during navigation.
        if (!string.IsNullOrWhiteSpace(readySelector))
        {
            try
            {
                await page.Locator(readySelector).First.WaitForAsync(new()
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = readyTimeoutMs
                }).ConfigureAwait(false);
            }
            catch (TimeoutException)
            {
                // Didn’t see the ready selector — return whatever we have so the caller can decide (parser can check).
            }
        }

        return await SafeContentAsync(page).ConfigureAwait(false);
    }

    // If the page is changing right as we read content, retry briefly.
    private static async Task<string> SafeContentAsync(IPage page)
    {
        for (int i = 0; i < 5; i++)
        {
            try { return await page.ContentAsync().ConfigureAwait(false); }
            catch (PlaywrightException ex)
                when (ex.Message.Contains("Execution context was destroyed", StringComparison.OrdinalIgnoreCase))
            { await Task.Delay(200).ConfigureAwait(false); }
        }
        return await page.ContentAsync().ConfigureAwait(false);
    }
}
