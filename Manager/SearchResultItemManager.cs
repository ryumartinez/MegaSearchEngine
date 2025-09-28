
using Manager.Contract;
using Microsoft.Extensions.DependencyInjection;

namespace Manager;

public class SearchResultItemManager() : ISearchResultItemManager
{
    public Task<IEnumerable<SearchResultItemModel>> GetAsync(string searchText)
    {
        throw new NotImplementedException();
    }
}