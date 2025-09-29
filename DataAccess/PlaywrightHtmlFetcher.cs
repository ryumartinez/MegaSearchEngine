using DataAccess.Contract;
using DataAccess.Infrastructure;
using Microsoft.Playwright;

namespace DataAccess;

public sealed class PlaywrightHtmlFetcher(IBrowserFactory browserFactory) : IHtmlFetcher
{
    public async Task<string> GetContentAsync(Uri url, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(url);
        var page = await browserFactory.CreatePageAsync().ConfigureAwait(false);
        page.SetDefaultTimeout(15000);
        await page.GotoAsync(url.ToString(), new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded }).ConfigureAwait(false);
        return await page.ContentAsync().ConfigureAwait(false);
    }

    public async Task<(string html, Uri? nextPage)> GetPagedContentAsync(Uri url, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(url);
        var page = await browserFactory.CreatePageAsync().ConfigureAwait(false);
        page.SetDefaultTimeout(15000);
        await page.GotoAsync(url.ToString(), new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded }).ConfigureAwait(false);

        // read HTML
        var html = await page.ContentAsync().ConfigureAwait(false);

        // try to discover "next" by rel or class
        var nextHref = await page
            .Locator("a[rel='next'], a.next")
            .First
            .GetAttributeAsync("href")
            .ConfigureAwait(false);        var next = string.IsNullOrWhiteSpace(nextHref) ? null : new Uri(new Uri(url.GetLeftPart(UriPartial.Authority)), nextHref);
        return (html, next);
    }
}
