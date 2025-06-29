using DataAccess.Contract.SearchResultItem;

namespace DataAccess;

public class DuckDuckGoSearchResultItemDataAccess : ISearchResultItemDataAccess
{
    public Task<IEnumerable<SearchResultItemAccessModel>> GetAsync(GetSearchResultItemAccessRequest request)
    {
        throw new NotImplementedException();
    }
}