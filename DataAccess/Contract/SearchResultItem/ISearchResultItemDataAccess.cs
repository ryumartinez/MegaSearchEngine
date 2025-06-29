namespace DataAccess.Contract.SearchResultItem;

public interface ISearchResultItemDataAccess
{ 
    Task<IEnumerable<SearchResultItemAccessModel>> GetAsync(GetSearchResultItemAccessRequest request);
}