namespace Manager.Contract;

public interface ISearchResultItemManager
{
    Task<GetSearchResultItemResponse> GetAsync(GetSearchResultItemRequest request);
    Task SearchAndSaveAsync(string searchText);
}