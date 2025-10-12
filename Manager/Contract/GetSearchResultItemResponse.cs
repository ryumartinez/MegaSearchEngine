namespace Manager.Contract;

public record GetSearchResultItemResponse(
    int TotalItems,
    int TotalBiggieItems,
    int TotalPuntoFarmaItems,
    int TotalFarmaTotalItems,
    IEnumerable<SearchResultItemModel> Items);