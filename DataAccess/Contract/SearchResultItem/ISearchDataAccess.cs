namespace DataAccess.Contract.SearchResultItem;

public interface ISearchDataAccess
{ 
    Task<IEnumerable<SearchResultItemAccessModel>> SearchAsync(SearchAccessRequest request);
}