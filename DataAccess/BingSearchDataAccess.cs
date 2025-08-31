using System.Net.Http;
using System.Web;
using DataAccess.Contract.SearchResultItem;
using Microsoft.Extensions.Configuration;

namespace DataAccess;

public class BingSearchDataAccess : ISearchDataAccess
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public BingSearchDataAccess(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _apiKey = config["Bing:ApiKey"] ?? throw new InvalidOperationException("Bing API key not configured.");
        _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _apiKey);
    }

    public async Task<IEnumerable<SearchResultItemAccessModel>> SearchAsync(SearchAccessRequest request)
    {
        var url = BuildSearchUrl(request.SearchText);
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        // TODO: Deserialize and map to SearchResultItemAccessModel
        return Enumerable.Empty<SearchResultItemAccessModel>();
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