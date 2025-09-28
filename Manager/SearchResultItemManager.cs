using Engine.Contract;
using Engine.Models;
using Manager.Contract;
using Microsoft.Extensions.DependencyInjection;

namespace Manager;

public class SearchResultItemManager(
    [FromKeyedServices("puntoFarma")] IEcommerceSearchScrapperEngine puntoFarmaSearchDataAccess,
    [FromKeyedServices("farmaTotal")] IEcommerceSearchScrapperEngine farmaTotalSearchDataAccess,
    [FromKeyedServices("biggie")] IEcommerceSearchScrapperEngine biggieSearchDataAccess
    ) : ISearchResultItemManager
{
    public async Task<IEnumerable<SearchResultItemModel>> GetAsync(string searchText)
    {
        var searchTasks = new List<Task<IEnumerable<EcommerceProductEngineModel>>>
        {
            puntoFarmaSearchDataAccess.ScrappeSearchAsync(searchText),
            farmaTotalSearchDataAccess.ScrappeSearchAsync(searchText),
            biggieSearchDataAccess.ScrappeSearchAsync(searchText),
        };

        var searchResultsFromAllProviders = await Task.WhenAll(searchTasks).ConfigureAwait(false);
        
        var mappedResult = searchResultsFromAllProviders
            .SelectMany(resultList => resultList)
            .Select(x => 
                new SearchResultItemModel(
                    x.Title,
                    x.Description,
                    x.Link
                ));

        return mappedResult;
    }
}