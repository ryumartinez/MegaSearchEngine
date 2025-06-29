namespace DataAccess.Contract.SearchResultItem;

public interface ISearchResultItemDataAccess
{ 
    Task<IEnumerable<SearchResultArticleAccessModel>> GetArticlesAsync(GetSearchResultItemAccessRequest request);
    
    Task<IEnumerable<SearchResultImageAccessModel>> GetImagesAsync(GetSearchResultItemAccessRequest request);
    
    Task<IEnumerable<SearchResultVideoAccessModel>> GetVideosAsync(GetSearchResultItemAccessRequest request);
}