using System.Net.Http;
using System.Text.Json;
using DataAccess.Contract.SearchResultItem;
using Microsoft.Extensions.Configuration;

namespace DataAccess;

public class DuckDuckGoSearchResultItemDataAccess : ISearchResultItemDataAccess
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public DuckDuckGoSearchResultItemDataAccess(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _apiKey = config["SerpApi:ApiKey"] ?? throw new InvalidOperationException("SerpAPI key not configured.");
    }

    public async Task<IEnumerable<SearchResultArticleAccessModel>> GetArticlesAsync(GetSearchResultItemAccessRequest request)
    {
        var json = await FetchSearchResultsAsync(request, "duckduckgo");
        // TODO: Map results to SearchResultArticleAccessModel
        return Enumerable.Empty<SearchResultArticleAccessModel>();
    }

    public async Task<IEnumerable<SearchResultImageAccessModel>> GetImagesAsync(GetSearchResultItemAccessRequest request)
    {
        var json = await FetchSearchResultsAsync(request, "duckduckgo_images");
        // TODO: Map results to SearchResultImageAccessModel
        return Enumerable.Empty<SearchResultImageAccessModel>();
    }

    public async Task<IEnumerable<SearchResultVideoAccessModel>> GetVideosAsync(GetSearchResultItemAccessRequest request)
    {
        var json = await FetchSearchResultsAsync(request, "duckduckgo_videos");
        // TODO: Map results to SearchResultVideoAccessModel
        return Enumerable.Empty<SearchResultVideoAccessModel>();
    }

    private async Task<string> FetchSearchResultsAsync(GetSearchResultItemAccessRequest request, string engine)
    {
        var url = $"https://serpapi.com/search.json?q={Uri.EscapeDataString(request.SearchText)}" +
                  $"&engine={engine}&api_key={_apiKey}&start={request.PageIndex}&num={request.PageSize}";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }
}
