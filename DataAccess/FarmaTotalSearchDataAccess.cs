using DataAccess.Contract.SearchResultItem;

namespace DataAccess;

public class FarmaTotalSearchDataAccess : ISearchDataAccess
{
    public Task<IEnumerable<SearchResultItemAccessModel>> SearchAsync(SearchAccessRequest request)
    {
        throw new NotImplementedException();
    }
}