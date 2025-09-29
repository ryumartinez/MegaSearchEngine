using Engine.Contract;
using Engine.Infrastructure;
using Engine.Models;

namespace Engine.EcommerceSearchScrappers;

//TODO: Bypass Site Block
public class FarmacenterSearchScrapperEngine(IBrowserFactory browser) : IEcommerceSearchScrapperEngine
{
    private const string BaseUrl = "https://www.farmacenter.com.py";

    public async Task<IEnumerable<EcommerceProductEngineModel>> ScrappeSearchAsync(string searchTerm)
    {
        var page = await browser.CreatePageAsync().ConfigureAwait(false);
        var searchUrl = $"{BaseUrl}/catalogo?q={Uri.EscapeDataString(searchTerm)}";
        await page.GotoAsync(searchUrl).ConfigureAwait(false);

        // Wait for the first product card to be visible on the page.
        await page.WaitForSelectorAsync("div.product").ConfigureAwait(false);

        var resultElements = await page.QuerySelectorAllAsync("div.product").ConfigureAwait(false);
            
        var searchResults = new List<EcommerceProductEngineModel>();
        foreach (var element in resultElements)
        {
            // Extract the title and the relative link from the main product link element.
            var linkElement = await element.QuerySelectorAsync("a.ecommercepro-LoopProduct-link").ConfigureAwait(false);
            var title = linkElement is not null ? await linkElement.GetAttributeAsync("title").ConfigureAwait(false) : string.Empty;
            var relativeLink = linkElement is not null ? await linkElement.GetAttributeAsync("href").ConfigureAwait(false) : string.Empty;

            // Extract the final price, which is inside a span with class 'price'.
            var priceElement = await element.QuerySelectorAsync("span.price span.amount").ConfigureAwait(false);
            var priceText = priceElement is not null ? await priceElement.InnerTextAsync().ConfigureAwait(false) : string.Empty;
                
            // Use the extracted price as the description.
            var description = priceText;
                
            if (!string.IsNullOrWhiteSpace(relativeLink) && !string.IsNullOrWhiteSpace(title))
            {
                searchResults.Add(new EcommerceProductEngineModel(
                    Title: title.Trim(),
                    Description: description.Trim(),
                    // Combine the base URL with the relative link to form a full, clickable URL.
                    Link: $"{BaseUrl}/{relativeLink}"
                ));
            }
        }
        return searchResults;
    }
}