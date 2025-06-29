using Manager.Contract;

namespace Manager;

public class SearchResultItemManager : ISearchResultItemManager
{
    public Task<IEnumerable<SearchResultItemModel>> GetAsync(GetSearchResultItemRequest request)
    {
        throw new NotImplementedException();
    }
}