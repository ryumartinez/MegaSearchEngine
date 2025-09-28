using Engine.Contract;
using Engine.Infrastructure;
using Engine.Models;

namespace Engine.EcommerceSearchScrappers;

public class PuntoFarmaSearchSearchScrapper(IBrowserFactory browser) : IEcommerceSearchScrapperEngine
{
    private const string BaseUrl = "https://www.puntofarma.com.py";

    public async Task<IEnumerable<EcommerceProductEngineModel>> ScrappeSearchAsync(string searchTerm)
    {
        var page = await browser.CreatePageAsync().ConfigureAwait(false);
        var searchUrl = $"{BaseUrl}/buscar?s={Uri.EscapeDataString(searchTerm)}";
        await page.GotoAsync(searchUrl).ConfigureAwait(false);
        await page.WaitForSelectorAsync("div.card-producto_cardProducto__Jl8Pw").ConfigureAwait(false);

        var resultElements = await page.QuerySelectorAllAsync("div.card-producto_cardProducto__Jl8Pw").ConfigureAwait(false);
        
        var searchResults = new List<EcommerceProductEngineModel>();
        foreach (var element in resultElements)
        {
            var titleElement = await element.QuerySelectorAsync("h2.card-title").ConfigureAwait(false);
            var linkElement = await (titleElement?.QuerySelectorAsync("xpath=ancestor::a")!).ConfigureAwait(false);
            
            var title = await (titleElement?.InnerTextAsync()!).ConfigureAwait(false);
            var relativeLink = await (linkElement?.GetAttributeAsync("href")!).ConfigureAwait(false);
            
            var priceAfterDiscountElement = await element.QuerySelectorAsync("span.precios_precioConDescuentoConPromoForma__2f14y").ConfigureAwait(false);
            var normalPrice = await element.QuerySelectorAsync("del.precios_precioSinDescuento__O97at").ConfigureAwait(false);
            var priceAfterDiscountText = priceAfterDiscountElement is not null ? await priceAfterDiscountElement.InnerTextAsync().ConfigureAwait(false) : string.Empty;
            var normalPriceText = normalPrice is not null ? await normalPrice.InnerTextAsync().ConfigureAwait(false) : string.Empty;
            
            var description = $"Precio sin descuento: {normalPriceText} , Precio con descuento:{priceAfterDiscountText}";
            
            if (!string.IsNullOrWhiteSpace(relativeLink) && !string.IsNullOrWhiteSpace(title))
            {
                searchResults.Add(new EcommerceProductEngineModel(
                    Title: title.Trim(),
                    Description: description.Trim(),
                    Link: $"{BaseUrl}{relativeLink}"
                ));
            }
        }
        return searchResults;
    }
}