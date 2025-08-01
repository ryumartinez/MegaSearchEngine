using DataAccess.Contract.SearchResultItem;

namespace DataAccess;

public class PuntoFarmaSearchDataAccess : ISearchDataAccess
{
    public Task<IEnumerable<SearchResultAccessModel>> SearchAsync(SearchAccessRequest request)
    {
        throw new NotImplementedException();
    }
}