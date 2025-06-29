namespace Manager.Contract;

public interface ISearchResultItemManager
{
    Task<IEnumerable<SearchResultItemModel>> GetAsync(GetSearchResultItemRequest request);
}