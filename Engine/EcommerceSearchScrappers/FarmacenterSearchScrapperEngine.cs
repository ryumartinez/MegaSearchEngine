using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Engine.Contract;
using Engine.Models;

namespace Engine.EcommerceSearchScrappers;

//TODO: Bypass Site Block
public sealed class FarmacenterParserEngine : IEcommerceParserEngine
    {
        private readonly HtmlParser _parser = new();

        // Primary selectors (with a couple of fallbacks in case classes change)
        private static readonly string[] CardSelectors =
        {
            "div.product",                      // primary
            "li.product",                       // common Woo-like fallback
            "div[class*='product']"             // broad fallback
        };

        private static readonly string[] LinkSelectors =
        {
            "a.ecommercepro-LoopProduct-link",  // primary from your scraper
            "a.woocommerce-LoopProduct-link",
            "a[href*='/producto/'], a[href*='/product/']",
            "a[href]"
        };

        // Price: primary amount selector + common fallbacks
        private static readonly string[] PriceSelectors =
        {
            "span.price span.amount",           // primary from your scraper
            "span.price ins .amount",           // promo price (ins)
            "span.price .amount",               // generic amount
            ".price"                            // broad fallback
        };

        public IEnumerable<EcommerceProductEngineModel> ParseSearchHtml(string html, Uri pageUrl)
        {
            if (string.IsNullOrWhiteSpace(html))
                yield break;

            var doc = _parser.ParseDocument(html);

            foreach (var card in QueryFirstAvailable(doc, CardSelectors))
            {
                // Link & title
                var a = FirstMatch(card, LinkSelectors) as IHtmlAnchorElement;
                var title = Clean(a?.Title) ?? Clean(a?.TextContent);
                var href  = a?.Href ?? a?.GetAttribute("href");

                if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(href))
                    continue;

                var link = ResolveUrl(pageUrl, href!);

                // Price text (keep raw for description)
                var priceText = Clean(TextOfFirst(card, PriceSelectors)) ?? string.Empty;

                yield return new EcommerceProductEngineModel(
                    Title: title!,
                    Description: priceText,
                    Link: link
                );
            }
        }

        // ----------------- helpers -----------------

        private static IEnumerable<IElement> QueryFirstAvailable(IParentNode scope, params string[] selectors)
        {
            foreach (var sel in selectors)
            {
                var found = scope.QuerySelectorAll(sel);
                if (found.Length > 0) return found;
            }
            return Array.Empty<IElement>();
        }

        private static IElement? FirstMatch(IElement scope, string[] selectors)
        {
            foreach (var sel in selectors)
            {
                var el = scope.QuerySelector(sel);
                if (el != null) return el;
            }
            return null;
        }

        private static string? TextOfFirst(IElement scope, string[] selectors)
        {
            foreach (var sel in selectors)
            {
                var el = scope.QuerySelector(sel);
                if (el != null) return el.TextContent;
            }
            return null;
        }

        private static string ResolveUrl(Uri pageUrl, string href)
        {
            if (Uri.TryCreate(href, UriKind.Absolute, out var abs)) return abs.ToString();
            return new Uri(pageUrl, href).ToString();
        }

        private static string? Clean(string? s)
            => string.IsNullOrWhiteSpace(s) ? null : s.Replace("\u00A0", " ", StringComparison.InvariantCulture).Trim();
        
    }