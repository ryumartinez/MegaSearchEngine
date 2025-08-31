using DataAccess.Contract.SearchResultItem;
using Manager.Contract;
using Microsoft.Extensions.DependencyInjection;

namespace Manager;

public class SearchResultItemManager(
    [FromKeyedServices("puntofarma")] ISearchDataAccess puntoFarmaSearchDataAccess,
    [FromKeyedServices("farmatotal")] ISearchDataAccess farmaTotalSearchDataAccess,
    [FromKeyedServices("biggie")] ISearchDataAccess biggieSearchDataAccess
    ) : ISearchResultItemManager
{
    public async Task<IEnumerable<SearchResultItemModel>> GetAsync(string searchText)
    {
        var request = new SearchAccessRequest(1, 1, searchText);

        // 1. Create a list of all the search tasks without awaiting them.
        // This starts all the operations concurrently.
        var searchTasks = new List<Task<IEnumerable<SearchResultItemAccessModel>>>
        {
            puntoFarmaSearchDataAccess.SearchAsync(request),
            farmaTotalSearchDataAccess.SearchAsync(request),
            biggieSearchDataAccess.SearchAsync(request),
        };

        // 2. Await all tasks to complete in parallel.
        var searchResultsFromAllProviders = await Task.WhenAll(searchTasks);

        // 3. Flatten the array of lists into a single list and map the results.
        var mappedResult = searchResultsFromAllProviders
            .SelectMany(resultList => resultList) // Flattens the results
            .Select(x => 
                new SearchResultItemModel(
                    x.Title,
                    x.Description,
                    x.Link
                ));

        return mappedResult;
    }
}