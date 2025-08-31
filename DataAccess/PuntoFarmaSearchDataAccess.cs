using DataAccess.Contract.SearchResultItem;
using DataAccess.Infrastructure;
using HtmlAgilityPack;
using Microsoft.Playwright;

namespace DataAccess;

public class PuntoFarmaSearchDataAccess(IBrowserFactory browser) : ISearchDataAccess
{
    private const string BaseUrl = "https://www.puntofarma.com.py";

    public async Task<IEnumerable<SearchResultItemAccessModel>> SearchAsync(SearchAccessRequest request)
    {
        IPage? page = null;
        try
        {
            page = await browser.CreatePageAsync();
            var searchUrl = $"{BaseUrl}/buscar?s={Uri.EscapeDataString(request.SearchText)}";
            await page.GotoAsync(searchUrl);

            // Wait for the first product card to be visible on the page.
            // The class name "card-producto_cardProducto__Jl8Pw" seems to be a stable identifier for product cards.
            await page.WaitForSelectorAsync("div.card-producto_cardProducto__Jl8Pw");

            var resultElements = await page.QuerySelectorAllAsync("div.card-producto_cardProducto__Jl8Pw");
            
            var searchResults = new List<SearchResultItemAccessModel>();
            foreach (var element in resultElements)
            {
                // The title and link are within the same <a> tag containing an <h2>
                var titleElement = await element.QuerySelectorAsync("h2.card-title");
                var linkElement = await titleElement?.QuerySelectorAsync("xpath=ancestor::a");
                
                var title = await titleElement?.InnerTextAsync() ?? string.Empty;
                var relativeLink = await linkElement?.GetAttributeAsync("href") ?? string.Empty;

                // The product cards don't have a separate description, so we'll use the title.
                var description = title;
                
                if (!string.IsNullOrWhiteSpace(relativeLink) && !string.IsNullOrWhiteSpace(title))
                {
                    searchResults.Add(new SearchResultItemAccessModel(
                        Title: title.Trim(),
                        Description: description.Trim(),
                        // Combine the base URL with the relative link to form a full, clickable URL.
                        Link: $"{BaseUrl}{relativeLink}"
                    ));
                }
            }
            return searchResults;
        }
        catch (TimeoutException)
        {
            Console.WriteLine("A timeout occurred waiting for Punto Farma results. Saving a screenshot for debugging...");
            if (page != null)
            {
                var path = Path.Combine(Path.GetTempPath(), "puntofarma_error_screenshot.png");
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