using DataAccess.Contract.SearchResultItem;

namespace DataAccess;

public class DuckDuckGoSearchResultItemDataAccess : ISearchResultItemDataAccess
{
    public Task<IEnumerable<SearchResultArticleAccessModel>> GetArticlesAsync(GetSearchResultItemAccessRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<SearchResultImageAccessModel>> GetImagesAsync(GetSearchResultItemAccessRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<SearchResultVideoAccessModel>> GetVideosAsync(GetSearchResultItemAccessRequest request)
    {
        throw new NotImplementedException();
    }
}