using DataAccess.Contract;
using Manager.Contract;

namespace Manager;

public class SearchManager(IProductDataAccess productDataAccess ) : ISearchManager
{
    private const string PuntoFarmaSiteName = "PuntoFarma";
    private const string FarmaTotalSiteName = "FarmaTotal";
    private const string BiggieSiteName = "Biggie";

    public async Task<GetSearchResultItemResponse> GetAsync(GetSearchResultItemRequest request)
    {
        var products = await productDataAccess.GetAsync(new GetProductAccessRequest()).ConfigureAwait(false);
        var items = products.Select(x => new SearchResultItemModel(x.Title, x.Description, x.Link, x.SiteName));
        var searchResultItemModels = items as SearchResultItemModel[] ?? items.ToArray();
        var result = new GetSearchResultItemResponse(
            TotalItems: searchResultItemModels.Length,
            TotalBiggieItems: searchResultItemModels.Count(x => x.Title == BiggieSiteName),
            TotalPuntoFarmaItems: searchResultItemModels.Count(x => x.Title == PuntoFarmaSiteName),
            TotalFarmaTotalItems: searchResultItemModels.Count(x => x.Title == FarmaTotalSiteName),
            Items: searchResultItemModels
            );
        return result;
    }
}