namespace Manager.Contract;

public interface ISearchResultItemManager
{
    Task<IEnumerable<SearchResultItemModel>> GetAsync(string searchText);
    Task SyncAsync();
}