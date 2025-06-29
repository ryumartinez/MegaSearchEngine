namespace DataAccess.Contract.SearchResultItem;

public record GetSearchResultItemAccessRequest(
    string PageSize,
    string PageIndex,
    string SearchText
    );