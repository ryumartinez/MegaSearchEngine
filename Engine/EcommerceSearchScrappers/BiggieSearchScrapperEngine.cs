using Engine.Contract;
using Engine.Infrastructure;
using Engine.Models;

namespace Engine.EcommerceSearchScrappers;

public class BiggieSearchScrapperEngine(IBrowserFactory browser) : IEcommerceSearchScrapperEngine
{
    private const string BaseUrl = "https://biggie.com.py";

    public async Task<IEnumerable<EcommerceProductEngineModel>> ScrappeSearchAsync(string searchTerm)
    {
        var page = await browser.CreatePageAsync().ConfigureAwait(false);
        var searchUrl = $"{BaseUrl}/search?q={Uri.EscapeDataString(searchTerm)}&c=0#result";
        await page.GotoAsync(searchUrl).ConfigureAwait(false);

        // Wait for the first product card to be visible. The site is a SPA, so we need to wait for JS to render the content.
        await page.WaitForSelectorAsync("div.card-container").ConfigureAwait(false);

        var resultElements = await page.QuerySelectorAllAsync("div.card-container").ConfigureAwait(false);
            
        var searchResults = new List<EcommerceProductEngineModel>();
        foreach (var element in resultElements)
        {
            // Extract the title
            var titleElement = await element.QuerySelectorAsync("div.v-card__title.titleCard").ConfigureAwait(false);
            var title = titleElement is not null ? await titleElement.InnerTextAsync().ConfigureAwait(false) : string.Empty;

            // Extract the final discounted price. It's the second span inside the price container div.
            var priceElement = await element.QuerySelectorAsync("div.v-card__text span:nth-child(2)").ConfigureAwait(false);
            var priceText = priceElement is not null ? await priceElement.InnerTextAsync().ConfigureAwait(false) : string.Empty;
                
            // Use the extracted price as the description.
            var description = priceText;
                
            // This site doesn't use traditional <a> tags with hrefs for product links on the search page.
            // We'll use the search URL as a placeholder link.
            if (!string.IsNullOrWhiteSpace(title))
            {
                searchResults.Add(new EcommerceProductEngineModel(
                    Title: title.Trim(),
                    Description: description.Trim(),
                    Link: searchUrl 
                ));
            }
        }
        return searchResults;
    }
}