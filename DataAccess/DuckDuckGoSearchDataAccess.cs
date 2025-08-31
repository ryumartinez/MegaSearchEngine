using DataAccess.Contract.SearchResultItem;
using DataAccess.Infrastructure;
using Microsoft.Playwright;

namespace DataAccess;

public class DuckDuckGoSearchDataAccess(IBrowserFactory browser) : ISearchDataAccess
{
    public async Task<IEnumerable<SearchResultItemAccessModel>> SearchAsync(SearchAccessRequest request)
    {
        // Declare the page variable here so it's accessible in the catch block
        IPage? page = null; 
        try
        {
            page = await browser.CreatePageAsync();
            await page.GotoAsync("https://duckduckgo.com/");

            await page.Locator("[name=\"q\"]").FillAsync(request.SearchText);
            await page.Keyboard.PressAsync("Enter");

            // We will still try to wait for the selector
            await page.WaitForSelectorAsync("article[id^=\"r1-\"]");

            var resultElements = await page.QuerySelectorAllAsync("article[id^=\"r1-\"]");
            
            var searchResults = new List<SearchResultItemAccessModel>();
            foreach (var element in resultElements)
            {
                var linkElement = await element.QuerySelectorAsync("h2 > a");
                var link = await linkElement?.GetAttributeAsync("href") ?? string.Empty;
                var title = await linkElement?.InnerTextAsync() ?? string.Empty;

                var descriptionElement = await element.QuerySelectorAsync("div[data-result=\"snippet\"]");
                var description = await descriptionElement?.InnerTextAsync() ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(link) && !string.IsNullOrWhiteSpace(title))
                {
                    searchResults.Add(new SearchResultItemAccessModel(
                        Title: title.Trim(),
                        Description: description.Trim(),
                        Link: link
                    ));
                }
            }
            return searchResults;
        }
        catch (TimeoutException ex)
        {
            Console.WriteLine("A timeout occurred. Saving a screenshot for debugging...");
            if (page != null)
            {
                var path = Path.Combine(Path.GetTempPath(), "duckduckgo_error_screenshot.png");
                await page.ScreenshotAsync(new() { Path = path, FullPage = true });
                Console.WriteLine($"Screenshot saved to: {path}");
            }
            // Re-throw the original exception to maintain the failure behavior
            throw; 
        }
        finally
        {
            // Ensure the page is closed even if an error occurs
            if (page != null)
            {
                await page.CloseAsync();
            }
        }
    }
}


