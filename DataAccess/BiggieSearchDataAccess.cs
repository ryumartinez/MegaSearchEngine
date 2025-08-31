using DataAccess.Contract.SearchResultItem;
using DataAccess.Infrastructure;
using Microsoft.Playwright;

namespace DataAccess;

public class BiggieSearchDataAccess(IBrowserFactory browser) : ISearchDataAccess
{
    private const string BaseUrl = "https://biggie.com.py";

    public async Task<IEnumerable<SearchResultItemAccessModel>> SearchAsync(SearchAccessRequest request)
    {
        IPage? page = null;
        try
        {
            page = await browser.CreatePageAsync();
            var searchUrl = $"{BaseUrl}/search?q={Uri.EscapeDataString(request.SearchText)}&c=0#result";
            await page.GotoAsync(searchUrl);

            // Wait for the first product card to be visible. The site is a SPA, so we need to wait for JS to render the content.
            await page.WaitForSelectorAsync("div.card-container");

            var resultElements = await page.QuerySelectorAllAsync("div.card-container");
            
            var searchResults = new List<SearchResultItemAccessModel>();
            foreach (var element in resultElements)
            {
                // Extract the title
                var titleElement = await element.QuerySelectorAsync("div.v-card__title.titleCard");
                var title = await titleElement?.InnerTextAsync() ?? string.Empty;

                // Extract the final discounted price. It's the second span inside the price container div.
                var priceElement = await element.QuerySelectorAsync("div.v-card__text span:nth-child(2)");
                var priceText = await priceElement?.InnerTextAsync() ?? string.Empty;
                
                // Use the extracted price as the description.
                var description = priceText;
                
                // This site doesn't use traditional <a> tags with hrefs for product links on the search page.
                // We'll use the search URL as a placeholder link.
                if (!string.IsNullOrWhiteSpace(title))
                {
                    searchResults.Add(new SearchResultItemAccessModel(
                        Title: title.Trim(),
                        Description: description.Trim(),
                        Link: searchUrl 
                    ));
                }
            }
            return searchResults;
        }
        catch (TimeoutException)
        {
            Console.WriteLine("A timeout occurred waiting for Biggie results. Saving a screenshot for debugging...");
            if (page != null)
            {
                var path = Path.Combine(Path.GetTempPath(), "biggie_error_screenshot.png");
                await page.ScreenshotAsync(new() { Path = path, FullPage = true });
                Console.WriteLine($"Screenshot saved to: {path}");
            }
            throw;
        }
        finally
        {
            if (page != null)
            {
                await page.CloseAsync();
            }
        }
    }
}