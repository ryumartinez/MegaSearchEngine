namespace Manager.Contract;

public interface ISearchManager
{
    Task<GetSearchResultItemResponse> GetAsync(GetSearchResultItemRequest request);
}