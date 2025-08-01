using DataAccess.Contract.SearchResultItem;

namespace DataAccess;

public class FarmacenterSearchDataAccess : ISearchDataAccess
{
    public Task<IEnumerable<SearchResultAccessModel>> SearchAsync(SearchAccessRequest request)
    {
        throw new NotImplementedException();
    }
}