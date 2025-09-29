using System.Text.RegularExpressions;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Engine.Contract;
using Engine.Models;

namespace Engine.EcommerceSearchScrappers;

public sealed class PuntoFarmaParserEngine : IEcommerceParserEngine
    {
        private readonly HtmlParser _parser = new();

        // Price regex tolerant to "1.234,56" / "1234.56" / "1234"
        private static readonly Regex PriceNumberRegex =
            new(@"(?<!\d)(\d{1,3}(?:[.\s]\d{3})*(?:[.,]\d{2})|\d+(?:[.,]\d{1,2})?)(?!\d)",
                RegexOptions.Compiled);

        // Selectors (primary + fallbacks for hashed class changes)
        private static readonly string[] CardSelectors =
        {
            "div.card-producto_cardProducto__Jl8Pw",     // current hashed class
            "div.card-product",                          // generic fallback if available
            "div[class*='cardProducto']"                 // partial fallback
        };

        private const string TitleSel = "h2.card-title";

        // Prices: promo (inside <span>), normal (often in <del>)
        private static readonly string[] PromoPriceSelectors =
        {
            "span.precios_precioConDescuentoConPromoForma__2f14y",
            "span[class*='precioConDescuento']",
            "span[class*='precioPromo']"
        };

        private static readonly string[] NormalPriceSelectors =
        {
            "del.precios_precioSinDescuento__O97at",
            "del[class*='precioSinDescuento']",
            "del[class*='precioTachado']"
        };

        public IEnumerable<EcommerceProductEngineModel> ParseSearchHtml(string html, Uri pageUrl)
        {
            if (string.IsNullOrWhiteSpace(html))
                yield break;

            var doc = _parser.ParseDocument(html);

            foreach (var card in QueryFirstAvailable(doc, CardSelectors))
            {
                // Title
                var titleEl = card.QuerySelector(TitleSel);
                var title   = Clean(titleEl?.TextContent);
                if (string.IsNullOrWhiteSpace(title))
                    continue;

                // Link: anchor that wraps the title (walk ancestors to be robust)
                var linkEl = FindAncestorAnchor(titleEl) ?? card.QuerySelector("a[href]");
                var href   = (linkEl as IHtmlAnchorElement)?.Href ?? linkEl?.GetAttribute("href");
                if (string.IsNullOrWhiteSpace(href))
                    continue;

                var absoluteLink = ResolveUrl(pageUrl, href!);

                // Prices
                var promoText  = Clean(TextOfFirst(card, PromoPriceSelectors));
                var normalText = Clean(TextOfFirst(card, NormalPriceSelectors));

                // Optional: parse numeric price if you later want decimals/currency
                // var (raw, value) = ParsePrice(promoText ?? normalText);

                var description = BuildDescription(normalText, promoText);

                yield return new EcommerceProductEngineModel(
                    Title: title!,
                    Description: description,
                    Link: absoluteLink
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

        private static IElement? FindAncestorAnchor(IElement? start)
        {
            var el = start;
            while (el is not null)
            {
                if (el is IHtmlAnchorElement) return el;
                el = el.ParentElement;
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

        private static (string raw, decimal? value) ParsePrice(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return (string.Empty, null);
            var raw = Clean(text)!;
            var m = PriceNumberRegex.Match(raw);
            if (!m.Success) return (raw, null);

            var num = m.Groups[1].Value
                .Replace(".", string.Empty, StringComparison.InvariantCulture)   // 1.234,56 -> 1234,56
                .Replace(" ", string.Empty, StringComparison.InvariantCulture)
                .Replace(",", ".", StringComparison.InvariantCulture);           // 1234,56 -> 1234.56

            return decimal.TryParse(num, System.Globalization.NumberStyles.Any,
                                    System.Globalization.CultureInfo.InvariantCulture, out var d)
                ? (raw, d)
                : (raw, null);
        }

        private static string BuildDescription(string? normal, string? promo)
        {
            var parts = new List<string>(2);
            if (!string.IsNullOrWhiteSpace(normal)) parts.Add($"Precio sin descuento: {normal!.Trim()}");
            if (!string.IsNullOrWhiteSpace(promo))  parts.Add($"Precio con descuento: {promo!.Trim()}");
            return string.Join(" · ", parts);
        }
    }