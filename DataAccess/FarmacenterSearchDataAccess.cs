using DataAccess.Contract.SearchResultItem;
using DataAccess.Infrastructure;
using Microsoft.Playwright;

namespace DataAccess;

//TODO: Bypass Site Block
public class FarmacenterSearchDataAccess(IBrowserFactory browser) : ISearchDataAccess
{
    private const string BaseUrl = "https://www.farmacenter.com.py";

    public async Task<IEnumerable<SearchResultItemAccessModel>> SearchAsync(SearchAccessRequest request)
    {
        IPage? page = null;
        try
        {
            page = await browser.CreatePageAsync();
            var searchUrl = $"{BaseUrl}/catalogo?q={Uri.EscapeDataString(request.SearchText)}";
            await page.GotoAsync(searchUrl);

            // Wait for the first product card to be visible on the page.
            await page.WaitForSelectorAsync("div.product");

            var resultElements = await page.QuerySelectorAllAsync("div.product");
            
            var searchResults = new List<SearchResultItemAccessModel>();
            foreach (var element in resultElements)
            {
                // Extract the title and the relative link from the main product link element.
                var linkElement = await element.QuerySelectorAsync("a.ecommercepro-LoopProduct-link");
                var title = await linkElement?.GetAttributeAsync("title") ?? string.Empty;
                var relativeLink = await linkElement?.GetAttributeAsync("href") ?? string.Empty;

                // Extract the final price, which is inside a span with class 'price'.
                var priceElement = await element.QuerySelectorAsync("span.price span.amount");
                var priceText = await priceElement?.InnerTextAsync() ?? string.Empty;
                
                // Use the extracted price as the description.
                var description = priceText;
                
                if (!string.IsNullOrWhiteSpace(relativeLink) && !string.IsNullOrWhiteSpace(title))
                {
                    searchResults.Add(new SearchResultItemAccessModel(
                        Title: title.Trim(),
                        Description: description.Trim(),
                        // Combine the base URL with the relative link to form a full, clickable URL.
                        Link: $"{BaseUrl}/{relativeLink}"
                    ));
                }
            }
            return searchResults;
        }
        catch (TimeoutException)
        {
            Console.WriteLine("A timeout occurred waiting for Farmacenter results. Saving a screenshot for debugging...");
            if (page != null)
            {
                var path = Path.Combine(Path.GetTempPath(), "farmacenter_error_screenshot.png");
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