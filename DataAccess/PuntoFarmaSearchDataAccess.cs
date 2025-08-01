using DataAccess.Contract.SearchResultItem;
using HtmlAgilityPack;

namespace DataAccess;

public class PuntoFarmaSearchDataAccess : ISearchDataAccess
{
    public async Task<IEnumerable<SearchResultAccessModel>> SearchAsync(SearchAccessRequest request)
    {
        var baseUrl = "https://www.puntofarma.com.py/buscar"; // Adjust to the real search URL
        var searchUrl = $"{baseUrl}?q={Uri.EscapeDataString(request.SearchText)}&page={request.PageIndex}&size={request.PageSize}";

        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");

        var html = await httpClient.GetStringAsync(searchUrl);

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(html);

        var productNodes = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class, 'product')]");

        var results = productNodes.Select(node =>
        {
            var titleNode = node.SelectSingleNode(".//h2");
            var descNode = node.SelectSingleNode(".//p[contains(@class, 'description')]");
            var linkNode = node.SelectSingleNode(".//a");

            var title = titleNode?.InnerText.Trim() ?? "No Title";
            var description = descNode?.InnerText.Trim() ?? "No Description";
            var link = linkNode?.GetAttributeValue("href", "#");

            // Make link absolute if needed
            if (!string.IsNullOrWhiteSpace(link) && !link.StartsWith("http"))
            {
                link = "https://www.puntofarma.com.py" + link;
            }

            return new SearchResultAccessModel(title, description, link);
        });

        return results;
    }
}