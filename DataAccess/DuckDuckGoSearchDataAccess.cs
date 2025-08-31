using DataAccess.Contract.SearchResultItem;
using DataAccess.Infrastructure;

namespace DataAccess;

public class DuckDuckGoSearchDataAccess(IBrowserFactory browser) : ISearchDataAccess
{
    public async Task<IEnumerable<SearchResultItemAccessModel>> SearchAsync(SearchAccessRequest request)
    {
        // 1. Create a new page for this operation. `await using` ensures it's disposed.
        var page = await browser.CreatePageAsync();

        // 2. Navigate to the search engine.
        await page.GotoAsync("https://duckduckgo.com/");

        // 3. Type the search query and press Enter to submit.
        await page.Locator("[name=\"q\"]").FillAsync(request.SearchText);
        await page.Keyboard.PressAsync("Enter");

        // 4. Wait for the results container to be visible on the page.
        // This ensures we don't try to scrape before the results have loaded.
        await page.WaitForSelectorAsync("#links");

        // 5. Scrape the search result articles from the page.
        // DuckDuckGo uses <article> elements with a specific data-testid for organic results.
        var resultElements = await page.QuerySelectorAllAsync("article[data-testid=\"result\"]");
        
        var searchResults = new List<SearchResultItemAccessModel>();

        foreach (var element in resultElements)
        {
            // Extract the link and title. They are within an <h2><a> structure.
            var linkElement = await element.QuerySelectorAsync("h2 > a");
            var link = await linkElement?.GetAttributeAsync("href") ?? string.Empty;
            var title = await linkElement?.InnerTextAsync() ?? string.Empty;

            // Extract the description snippet.
            var descriptionElement = await element.QuerySelectorAsync("div[data-testid=\"result-snippet\"]");
            var description = await descriptionElement?.InnerTextAsync() ?? string.Empty;

            // Add the result to the list if it's a valid item (has a link and title).
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
}
