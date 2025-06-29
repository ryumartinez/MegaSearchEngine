using System.Net.Http;
using System.Web;
using DataAccess.Contract.SearchResultItem;
using Microsoft.Extensions.Configuration;

namespace DataAccess;

public class GoogleSearchResultItemDataAccess : ISearchResultItemDataAccess
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _cseId;

    public GoogleSearchResultItemDataAccess(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _apiKey = config["Google:ApiKey"] ?? throw new InvalidOperationException("Google API key not configured.");
        _cseId = config["Google:CseId"] ?? throw new InvalidOperationException("Google CSE ID not configured.");
    }

    public async Task<IEnumerable<SearchResultArticleAccessModel>> GetArticlesAsync(GetSearchResultItemAccessRequest request)
    {
        var url = BuildSearchUrl(request.SearchText, request.PageIndex, request.PageSize);
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        // TODO: Deserialize and map to SearchResultArticleAccessModel
        return Enumerable.Empty<SearchResultArticleAccessModel>();
    }

    public Task<IEnumerable<SearchResultImageAccessModel>> GetImagesAsync(GetSearchResultItemAccessRequest request)
    {
        // Use Google Images Search (same endpoint, add `searchType=image`)
        return Task.FromResult(Enumerable.Empty<SearchResultImageAccessModel>());
    }

    public Task<IEnumerable<SearchResultVideoAccessModel>> GetVideosAsync(GetSearchResultItemAccessRequest request)
    {
        // Not directly supported by CSE, usually handled via YouTube API
        return Task.FromResult(Enumerable.Empty<SearchResultVideoAccessModel>());
    }

    private string BuildSearchUrl(string query, string pageIndex, string pageSize)
    {
        var builder = new UriBuilder("https://www.googleapis.com/customsearch/v1");
        var queryString = HttpUtility.ParseQueryString(string.Empty);
        queryString["key"] = _apiKey;
        queryString["cx"] = _cseId;
        queryString["q"] = query;
        queryString["start"] = pageIndex;
        queryString["num"] = pageSize;
        builder.Query = queryString.ToString();
        return builder.ToString();
    }
}
