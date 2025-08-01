namespace DataAccess.Contract.SearchResultItem;

public interface ISearchDataAccess
{ 
    Task<IEnumerable<SearchResultAccessModel>> SearchAsync(SearchAccessRequest request);
}