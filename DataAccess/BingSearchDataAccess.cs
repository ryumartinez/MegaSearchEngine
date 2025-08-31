using System.Net.Http;
using System.Web;
using DataAccess.Contract.SearchResultItem;
using Microsoft.Extensions.Configuration;

namespace DataAccess;

public class BingSearchDataAccess : ISearchDataAccess
{
    public Task<IEnumerable<SearchResultItemAccessModel>> SearchAsync(SearchAccessRequest request)
    {
        throw new NotImplementedException();
    }
}