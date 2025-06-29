using DataAccess.Contract.SearchResultItem;

namespace DataAccess;

internal class GoogleSearchResultItemDataAccess : ISearchResultItemDataAccess
{
    public Task<IEnumerable<SearchResultItemAccessModel>> GetAsync(GetSearchResultItemAccessRequest request)
    {
        throw new NotImplementedException();
    }
}