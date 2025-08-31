using DataAccess.Contract.SearchResultItem;
using DataAccess.Infrastructure;

namespace DataAccess;

public class DuckDuckGoSearchDataAccess(IBrowserFactory browser) : ISearchDataAccess
{
    public async Task<IEnumerable<SearchResultItemAccessModel>> SearchAsync(SearchAccessRequest request)
    {
        var page = await browser.CreatePageAsync();

        await page.GotoAsync("https://duckduckgo.com/");

        await page.Locator("[name=\"q\"]").FillAsync(request.SearchText);
        await page.Keyboard.PressAsync("Enter");

        // --- FIX ---
        // The data-testid attribute is volatile. A more stable selector is the article's ID,
        // which follows a pattern like "r1-0", "r1-1", etc.
        // The selector 'article[id^="r1-"]' waits for the first article whose ID starts with "r1-".
        await page.WaitForSelectorAsync("article[id^=\"r1-\"]");

        // Use the same stable selector to get all result elements.
        var resultElements = await page.QuerySelectorAllAsync("article[id^=\"r1-\"]");
        
        var searchResults = new List<SearchResultItemAccessModel>();

        foreach (var element in resultElements)
        {
            // The selectors for the title and link within the article are likely still correct.
            var linkElement = await element.QuerySelectorAsync("h2 > a");
            var link = await linkElement?.GetAttributeAsync("href") ?? string.Empty;
            var title = await linkElement?.InnerTextAsync() ?? string.Empty;

            // The selector for the description snippet is also likely still correct.
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
}


