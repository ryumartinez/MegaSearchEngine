using System.Net.Http;
using System.Web;
using DataAccess.Contract.SearchResultItem;
using Microsoft.Extensions.Configuration;

namespace DataAccess;

public class BingSearchResultItemDataAccess : ISearchResultItemDataAccess
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public BingSearchResultItemDataAccess(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _apiKey = config["Bing:ApiKey"] ?? throw new InvalidOperationException("Bing API key not configured.");
        _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _apiKey);
    }

    public async Task<IEnumerable<SearchResultArticleAccessModel>> GetArticlesAsync(GetSearchResultItemAccessRequest request)
    {
        var url = BuildSearchUrl(request.SearchText);
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        // TODO: Deserialize and map to SearchResultArticleAccessModel
        return Enumerable.Empty<SearchResultArticleAccessModel>();
    }

    public Task<IEnumerable<SearchResultImageAccessModel>> GetImagesAsync(GetSearchResultItemAccessRequest request)
    {
        // Use Bing Image Search API
        return Task.FromResult(Enumerable.Empty<SearchResultImageAccessModel>());
    }

    public Task<IEnumerable<SearchResultVideoAccessModel>> GetVideosAsync(GetSearchResultItemAccessRequest request)
    {
        // Use Bing Video Search API
        return Task.FromResult(Enumerable.Empty<SearchResultVideoAccessModel>());
    }

    private string BuildSearchUrl(string query)
    {
        var builder = new UriBuilder("https://api.bing.microsoft.com/v7.0/search");
        var queryString = HttpUtility.ParseQueryString(string.Empty);
        queryString["q"] = query;
        builder.Query = queryString.ToString();
        return builder.ToString();
    }
}