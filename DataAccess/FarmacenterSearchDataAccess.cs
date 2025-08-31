using DataAccess.Contract.SearchResultItem;

namespace DataAccess;

public class FarmacenterSearchDataAccess : ISearchDataAccess
{
    public Task<IEnumerable<SearchResultItemAccessModel>> SearchAsync(SearchAccessRequest request)
    {
        throw new NotImplementedException();
    }
}