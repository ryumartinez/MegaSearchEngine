using System.Collections.Concurrent;
using DataAccess.Contract;
using Engine.Contract;
using Engine.Models;
using Manager.Contract;
using Microsoft.Extensions.DependencyInjection;

namespace Manager;

public class SearchResultItemManager(
    [FromKeyedServices("puntoFarma")] IEcommerceParserEngine puntoFarmaParser,
    [FromKeyedServices("farmaTotal")] IEcommerceParserEngine farmaTotalParser,
    [FromKeyedServices("biggie")] IEcommerceParserEngine biggieParser,
    IHtmlFetcher htmlFetcher,
    IProductDataAccess productDataAccess
) : ISearchResultItemManager
{
    private const string PuntoFarmaBase   = "https://www.puntofarma.com.py";
    private const string FarmaTotalBase   = "https://www.farmatotal.com.py";
    private const string BiggieBase       = "https://biggie.com.py";

    public async Task<IEnumerable<SearchResultItemModel>> GetAsync(string searchText)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(searchText);

        // Kick off all sites in parallel; each site failure is isolated.
        var tasks = new[]
        {
            FetchAndParseAsync(
                new Uri($"{PuntoFarmaBase}/buscar?s={Uri.EscapeDataString(searchText)}"),
                puntoFarmaParser),

            FetchAndParseAsync(
                new Uri($"{FarmaTotalBase}/?s={Uri.EscapeDataString(searchText)}&post_type=product"),
                farmaTotalParser),

            FetchAndParseAsync(
                new Uri($"{BiggieBase}/search?q={Uri.EscapeDataString(searchText)}&c=0#result"),
                biggieParser),
        };

        var resultsPerSite = await Task.WhenAll(tasks).ConfigureAwait(false);

        // Flatten + de-dupe by Link (stable key across runs). If Link is empty (shouldn't be), keep first by Title.
        var dedup = new ConcurrentDictionary<string, SearchResultItemModel>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in resultsPerSite.SelectMany(x => x))
        {
            var key = string.IsNullOrWhiteSpace(item.Link) ? item.Title : item.Link;
            dedup.TryAdd(key, item);
        }

        // You can add ordering (e.g., by Title)
        return dedup.Values.OrderBy(x => x.Title, StringComparer.OrdinalIgnoreCase);
    }

    public async Task SyncAsync()
    {
        // Choose your default sync query; keep it in config in real apps.
        const string defaultQuery = "ibuprofeno";

        var items = await GetAsync(defaultQuery).ConfigureAwait(false);

        // Map to DA requests and persist. If you add an upsert, call that here instead.
        var requests = items.Select(x => new CreateProductAccessRequest(
            Name: x.Title,
            Description: x.Description,
            Link: x.Link));

        await productDataAccess.AddRangeAsync(requests).ConfigureAwait(false);
    }

    // ----------------- helpers -----------------

    private async Task<IEnumerable<SearchResultItemModel>> FetchAndParseAsync(
        Uri url,
        IEcommerceParserEngine parser,
        CancellationToken ct = default)
    {
        var html = await htmlFetcher.GetContentAsync(url, ct).ConfigureAwait(false);
        var parsed = parser.ParseSearchHtml(html, url);

        return parsed.Select(p => new SearchResultItemModel(
            Title: p.Title,
            Description: p.Description,
            Link: p.Link));
    }
}