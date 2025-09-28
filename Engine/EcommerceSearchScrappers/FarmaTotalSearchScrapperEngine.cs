using Engine.Contract;
using Engine.Infrastructure;
using Engine.Models;
using Microsoft.Playwright;

namespace Engine.EcommerceSearchScrappers;

public class FarmaTotalSearchScrapperEngine(IBrowserFactory browser) : IEcommerceSearchScrapperEngine
{
    private const string BaseUrl = "https://www.farmatotal.com.py";

    public async Task<IEnumerable<EcommerceProductEngineModel>> ScrappeSearchAsync(string searchTerm)
    {
        var page = await browser.CreatePageAsync().ConfigureAwait(false);
        var searchUrl = $"{BaseUrl}/?s={Uri.EscapeDataString(searchTerm)}&post_type=product";
        await page.GotoAsync(searchUrl).ConfigureAwait(false);

        // Wait for the first product card to be visible on the page.
        await page.WaitForSelectorAsync("div.product").ConfigureAwait(false);

        var resultElements = await page.QuerySelectorAllAsync("div.product").ConfigureAwait(false);
            
        var searchResults = new List<EcommerceProductEngineModel>();
        foreach (var element in resultElements)
        {
            // Extract the title from the h3 tag
            var titleElement = await element.QuerySelectorAsync("h3.product-title a").ConfigureAwait(false);
            var title = titleElement is not null ? await titleElement.InnerTextAsync().ConfigureAwait(false) : string.Empty;
            var link = titleElement is not null ? await titleElement.GetAttributeAsync("href").ConfigureAwait(false) : string.Empty;

            // Extract the final "Web Price" which is inside an <ins> tag
            var priceElement = await element.QuerySelectorAsync("span.price ins span.woocommerce-Price-amount bdi").ConfigureAwait(false);
            var priceText = priceElement is not null ? await priceElement.InnerTextAsync().ConfigureAwait(false) : string.Empty;
                
            // Use the extracted price as the description.
            var description = priceText;
                
            if (!string.IsNullOrWhiteSpace(link) && !string.IsNullOrWhiteSpace(title))
            {
                searchResults.Add(new EcommerceProductEngineModel(
                    Title: title.Trim(),
                    Description: description.Trim(),
                    Link: link
                ));
            }
        }
        return searchResults;
    }
}