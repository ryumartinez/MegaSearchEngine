using System.Text.RegularExpressions;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Engine.Contract;
using Engine.Models;

namespace Engine.EcommerceSearchScrappers;

public sealed class BiggieParserEngine : IEcommerceParserEngine
    {
        private readonly HtmlParser _parser = new();

        // Some pages use these structures/classes (Vuetify-based)
        private static readonly string[] CardSelectors =
        {
            "div.card-container",           // primary seen in your scraper
            "div.v-card",                   // generic Vuetify card
            "div.product-card"              // extra fallback
        };

        private static readonly string[] TitleSelectors =
        {
            "div.v-card__title.titleCard",  // primary
            "div.v-card__title",            // fallback
            "h2, h3"                        // generic fallback
        };

        // Price container often holds two spans; the discounted price is the 2nd.
        // We also include fallbacks to any span with a currency sign.
        private static readonly string[] PriceSelectors =
        {
            "div.v-card__text span:nth-child(2)",
            "div.v-card__text span",
            "span[class*='price'], span:matches(^\\s*[₲$€R$]|\\d+[.,]\\d{2}\\s*[₲$€])"
        };

        public IEnumerable<EcommerceProductEngineModel> ParseSearchHtml(string html, Uri pageUrl)
        {
            ArgumentNullException.ThrowIfNull(pageUrl);
            ArgumentNullException.ThrowIfNull(html);
            
            var doc = _parser.ParseDocument(html);

            foreach (var card in QueryFirstAvailable(doc, CardSelectors))
            {
                // Title
                var titleText = Clean(TextOfFirst(card, TitleSelectors));
                if (string.IsNullOrWhiteSpace(titleText))
                    continue;

                // Price (prefer “discounted” position, then fallbacks)
                var priceText = Clean(TextOfFirst(card, PriceSelectors));

                // Link:
                // 1) anchor with href
                // 2) common SPA data attributes
                // 3) onclick/location handlers
                // 4) fallback: the current page URL
                var link = TryResolveLink(card, pageUrl);

                yield return new EcommerceProductEngineModel(
                    Title: titleText!,
                    Description: priceText ?? string.Empty,
                    Link: link
                );
            }
        }

        // ---------- helpers ---------------------------------------------------

        private static IEnumerable<IElement> QueryFirstAvailable(IParentNode scope, params string[] selectors)
        {
            foreach (var sel in selectors)
            {
                var found = scope.QuerySelectorAll(sel);
                if (found.Length > 0) return found;
            }
            return Array.Empty<IElement>();
        }

        private static string? TextOfFirst(IElement scope, string[] selectors)
        {
            foreach (var sel in selectors)
            {
                var el = scope.QuerySelector(sel);
                if (el != null)
                    return el.TextContent;
            }
            return null;
        }

        private static string TryResolveLink(IElement card, Uri pageUrl)
        {
            // 1) Direct anchor
            var a = card.QuerySelector("a[href]") as IHtmlAnchorElement;
            var href = a?.Href ?? a?.GetAttribute("href");
            if (!string.IsNullOrWhiteSpace(href))
                return ResolveUrl(pageUrl, href!);

            // 2) Data attributes sometimes used by SPAs
            href = card.GetAttribute("data-href")
                ?? card.GetAttribute("data-url")
                ?? card.GetAttribute("data-link");
            if (!string.IsNullOrWhiteSpace(href))
                return ResolveUrl(pageUrl, href!);

            // 3) Inline handlers (e.g., onclick="location.href='/product/xyz'")
            var onclick = card.GetAttribute("onclick");
            if (!string.IsNullOrWhiteSpace(onclick))
            {
                var m = Regex.Match(onclick, @"href\s*=\s*['""]([^'""]+)['""]");
                if (m.Success) return ResolveUrl(pageUrl, m.Groups[1].Value);
                m = Regex.Match(onclick, @"location\.href\s*=\s*['""]([^'""]+)['""]");
                if (m.Success) return ResolveUrl(pageUrl, m.Groups[1].Value);
            }

            // 4) Fallback: use page URL (keeps model valid)
            return pageUrl.ToString();
        }

        private static string ResolveUrl(Uri pageUrl, string href)
        {
            if (Uri.TryCreate(href, UriKind.Absolute, out var abs)) return abs.ToString();
            return new Uri(pageUrl, href).ToString();
        }

        private static string? Clean(string? s)
            => string.IsNullOrWhiteSpace(s) ? null : s.Replace("\u00A0", " ", StringComparison.InvariantCulture).Trim();
        
    }