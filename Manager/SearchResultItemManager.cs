using DataAccess.Contract.SearchResultItem;
using Manager.Contract;
using Microsoft.Extensions.DependencyInjection;

namespace Manager;

public class SearchResultItemManager(
    [FromKeyedServices("duckduckgo")] ISearchDataAccess duckDuckGoSearchDataAccess,
    [FromKeyedServices("puntofarma")] ISearchDataAccess puntoFarmaSearchDataAccess,
    [FromKeyedServices("farmacenter")] ISearchDataAccess farmaCenterSearchDataAccess,
    [FromKeyedServices("farmatotal")] ISearchDataAccess farmaTotalSearchDataAccess,
    [FromKeyedServices("biggie")] ISearchDataAccess biggieSearchDataAccess
    ) : ISearchResultItemManager
{
    public async Task<IEnumerable<SearchResultItemModel>> GetAsync(string searchText)
    {
        var request = new SearchAccessRequest(1,1, searchText);
        var result = await biggieSearchDataAccess.SearchAsync(request);
        var mappedResult = result
            .Select(x => 
                new SearchResultItemModel(
                    x.Title,
                    x.Description,
                    x.Link
                    ));
        return mappedResult;
    }
}