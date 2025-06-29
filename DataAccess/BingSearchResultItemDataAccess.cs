using DataAccess.Contract.SearchResultItem;

namespace DataAccess;

public class BingSearchResultItemDataAccess : ISearchResultItemDataAccess
{
    public Task<IEnumerable<SearchResultItemAccessModel>> GetAsync(GetSearchResultItemAccessRequest request)
    {
        throw new NotImplementedException();
    }
}