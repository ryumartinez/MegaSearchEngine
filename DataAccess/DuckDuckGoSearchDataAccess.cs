using System.Net.Http;
using System.Text.Json;
using DataAccess.Contract.SearchResultItem;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;

namespace DataAccess;

public class DuckDuckGoSearchDataAccess(HttpClient httpClient) : ISearchDataAccess
{
    public async Task<IEnumerable<SearchResultAccessModel>> SearchAsync(SearchAccessRequest request)
    {
        var baseUrl = "https://html.duckduckgo.com/html/";
        var query = Uri.EscapeDataString(request.SearchText);
        var page = int.TryParse(request.PageIndex, out var pageIndex) ? pageIndex : 0;
        
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("q", request.SearchText),
        });

        // DuckDuckGo's HTML version requires POST for search
        var response = await httpClient.PostAsync(baseUrl, formData);
        var html = await response.Content.ReadAsStringAsync();

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(html);

        var resultNodes = htmlDoc.DocumentNode.SelectNodes("//div[@class='result']");

        var results = resultNodes.Select(node =>
        {
            var titleNode = node.SelectSingleNode(".//a[contains(@class,'result__a')]");
            var snippetNode = node.SelectSingleNode(".//a[contains(@class,'result__snippet')]");

            var title = titleNode?.InnerText?.Trim() ?? "No Title";
            var link = titleNode?.GetAttributeValue("href", "#") ?? "#";
            var description = snippetNode?.InnerText?.Trim() ?? "No Description";

            return new SearchResultAccessModel(title, description, link);
        });

        // Apply paging manually (not supported by URL param)
        return results.Skip(page * int.Parse(request.PageSize)).Take(int.Parse(request.PageSize));
    }
}
