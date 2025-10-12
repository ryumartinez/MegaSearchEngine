using Engine.Contract;
using Engine.Models;
using Microsoft.Playwright;

namespace Engine.EcommerceSearchScrappers;

public sealed class FarmaTotalParserEngine : IEcommerceParserEngine
{
    public IEnumerable<EcommerceProductEngineModel> ParseSearchHtml(string html, Uri pageUrl, string siteName)
    {
        var doc = new AngleSharp.Html.Parser.HtmlParser().ParseDocument(html);
        foreach (var card in doc.QuerySelectorAll("div.product"))
        {
            var a = card.QuerySelector("h3.product-title a");
            var title = a?.TextContent?.Trim();
            var link  = a?.GetAttribute("href");
            var priceNode = card.QuerySelector("span.price ins span.woocommerce-Price-amount bdi")
                            ?? card.QuerySelector("span.price span.woocommerce-Price-amount bdi");
            var priceText = priceNode?.TextContent?.Trim();

            if (!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(link))
                yield return new EcommerceProductEngineModel(
                    Title: title!,
                    Description: priceText ?? string.Empty,
                    Link: link!.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? link : new Uri(pageUrl, link).ToString(),
                    SiteName: siteName
                );
        }
    }
}