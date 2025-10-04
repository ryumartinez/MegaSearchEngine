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
    [FromKeyedServices("biggie")]     IEcommerceParserEngine biggieParser,
    IHtmlFetcher htmlFetcher,
    IProductDataAccess productDataAccess
) : ISearchResultItemManager
{
    private const string PuntoFarmaSiteName = "PuntoFarma";
    private const string FarmaTotalSiteName = "FarmaTotal";
    private const string BiggieSiteName = "Biggie";
    
    private const string PuntoFarmaBase = "https://www.puntofarma.com.py";
    private const string FarmaTotalBase = "https://www.farmatotal.com.py";
    private const string BiggieBase     = "https://biggie.com.py";

    // Site-specific “ready” selectors (what we wait for before reading HTML)
    private const string PuntoFarmaReadySelector = "div.card-producto_cardProducto__Jl8Pw";
    private const string FarmaTotalReadySelector = "div.product";
    private const string BiggieReadySelector     = "div.card-container";

    public async Task<GetSearchResultItemResponse> GetAsync(GetSearchResultItemRequest request)
    {
        var products = await productDataAccess.GetAsync(new GetProductAccessRequest()).ConfigureAwait(false);
        var items = products.Select(x => new SearchResultItemModel(x.Title, x.Description, x.Link, x.SiteName));
        var searchResultItemModels = items as SearchResultItemModel[] ?? items.ToArray();
        var result = new GetSearchResultItemResponse(
            TotalItems: searchResultItemModels.Length,
            TotalBiggieItems: searchResultItemModels.Count(x => x.Title == BiggieSiteName),
            TotalPuntoFarmaItems: searchResultItemModels.Count(x => x.Title == PuntoFarmaSiteName),
            TotalFarmaTotalItems: searchResultItemModels.Count(x => x.Title == FarmaTotalSiteName),
            Items: searchResultItemModels
            );
        return result;
    }

    public async Task SearchAndSaveAsync(string searchText)
    {
        var items = await FetchAndParseAsync(searchText).ConfigureAwait(false);

        var requests = items.Select(x => new CreateProductAccessRequest(
            Name: x.Title,
            Description: x.Description,
            Link: x.Link,
            SiteName: x.SiteName));

        await productDataAccess.AddRangeAsync(requests).ConfigureAwait(false);
    }
    
    private async Task<IEnumerable<SearchResultItemModel>> FetchAndParseAsync(string searchText)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(searchText);

        var tasks = new[]
        {
            FetchAndParseAsync(
                PuntoFarmaSiteName,
                new Uri($"{PuntoFarmaBase}/buscar?s={Uri.EscapeDataString(searchText)}"),
                PuntoFarmaReadySelector,
                15_000,
                puntoFarmaParser),

            FetchAndParseAsync(
                FarmaTotalSiteName,
                new Uri($"{FarmaTotalBase}/?s={Uri.EscapeDataString(searchText)}&post_type=product"),
                FarmaTotalReadySelector,
                20_000,
                farmaTotalParser),

            FetchAndParseAsync(
                BiggieSiteName,
                new Uri($"{BiggieBase}/search?q={Uri.EscapeDataString(searchText)}&c=0#result"),
                BiggieReadySelector,
                15_000,
                biggieParser),
        };

        var perSite = await Task.WhenAll(tasks).ConfigureAwait(false);

        // Flatten + de-dupe by Link (fallback to Title if link is empty)
        var bag = new ConcurrentDictionary<string, SearchResultItemModel>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in perSite.SelectMany(x => x))
        {
            var key = string.IsNullOrWhiteSpace(item.Link) ? item.Title : item.Link;
            bag.TryAdd(key, item);
        }

        // Optional ordering
        return bag.Values.OrderBy(x => x.Title, StringComparer.OrdinalIgnoreCase);
    }

    private async Task<IEnumerable<SearchResultItemModel>> FetchAndParseAsync(
        string siteName,
        Uri url,
        string readySelector,
        int readyTimeoutMs,
        IEcommerceParserEngine parser,
        CancellationToken ct = default)
    {
        var html = await htmlFetcher
            .GetContentAsync(url, readySelector, readyTimeoutMs, ct)
            .ConfigureAwait(false);

        var parsed = parser.ParseSearchHtml(html, url, siteName);

        return parsed.Select(p => new SearchResultItemModel(
            Title: p.Title,
            Description: p.Description,
            Link: p.Link,
            SiteName: p.SiteName));
    }
}