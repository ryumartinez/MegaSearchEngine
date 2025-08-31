using DataAccess.Contract.SearchResultItem;
using DataAccess.Infrastructure;
using Microsoft.Playwright;

namespace DataAccess;

public class FarmaTotalSearchDataAccess(IBrowserFactory browser) : ISearchDataAccess
{
    private const string BaseUrl = "https://www.farmatotal.com.py";

    public async Task<IEnumerable<SearchResultItemAccessModel>> SearchAsync(SearchAccessRequest request)
    {
        IPage? page = null;
        try
        {
            page = await browser.CreatePageAsync();
            var searchUrl = $"{BaseUrl}/?s={Uri.EscapeDataString(request.SearchText)}&post_type=product";
            await page.GotoAsync(searchUrl);

            // Wait for the first product card to be visible on the page.
            await page.WaitForSelectorAsync("div.product");

            var resultElements = await page.QuerySelectorAllAsync("div.product");
            
            var searchResults = new List<SearchResultItemAccessModel>();
            foreach (var element in resultElements)
            {
                // Extract the title from the h3 tag
                var titleElement = await element.QuerySelectorAsync("h3.product-title a");
                var title = await titleElement?.InnerTextAsync() ?? string.Empty;
                var link = await titleElement?.GetAttributeAsync("href") ?? string.Empty;

                // Extract the final "Web Price" which is inside an <ins> tag
                var priceElement = await element.QuerySelectorAsync("span.price ins span.woocommerce-Price-amount bdi");
                var priceText = await priceElement?.InnerTextAsync() ?? string.Empty;
                
                // Use the extracted price as the description.
                var description = priceText;
                
                if (!string.IsNullOrWhiteSpace(link) && !string.IsNullOrWhiteSpace(title))
                {
                    searchResults.Add(new SearchResultItemAccessModel(
                        Title: title.Trim(),
                        Description: description.Trim(),
                        Link: link // The link is already absolute
                    ));
                }
            }
            return searchResults;
        }
        catch (TimeoutException)
        {
            Console.WriteLine("A timeout occurred waiting for FarmaTotal results. Saving a screenshot for debugging...");
            if (page != null)
            {
                var path = Path.Combine(Path.GetTempPath(), "farmatotal_error_screenshot.png");
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