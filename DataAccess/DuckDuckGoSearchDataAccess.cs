using System.Net.Http;
using System.Text.Json;
using DataAccess.Contract.SearchResultItem;
using Microsoft.Extensions.Configuration;

namespace DataAccess;

public class DuckDuckGoSearchDataAccess : ISearchDataAccess
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public DuckDuckGoSearchDataAccess(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _apiKey = config["SerpApi:ApiKey"] ?? throw new InvalidOperationException("SerpAPI key not configured.");
    }

    public async Task<IEnumerable<SearchResultAccessModel>> SearchAsync(SearchAccessRequest request)
    {
        var json = await FetchSearchResultsAsync(request, "duckduckgo");
        // TODO: Map results to SearchResultAccessModel
        return Enumerable.Empty<SearchResultAccessModel>();
    }

    private async Task<string> FetchSearchResultsAsync(SearchAccessRequest request, string engine)
    {
        var url = $"https://serpapi.com/search.json?q={Uri.EscapeDataString(request.SearchText)}" +
                  $"&engine={engine}&api_key={_apiKey}&start={request.PageIndex}&num={request.PageSize}";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }
}
