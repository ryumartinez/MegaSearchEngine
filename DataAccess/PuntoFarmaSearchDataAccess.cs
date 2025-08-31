using DataAccess.Contract.SearchResultItem;
using HtmlAgilityPack;

namespace DataAccess;

public class PuntoFarmaSearchDataAccess(HttpClient httpClient) : ISearchDataAccess
{
    public Task<IEnumerable<SearchResultItemAccessModel>> SearchAsync(SearchAccessRequest request)
    {
        throw new NotImplementedException();
    }
}