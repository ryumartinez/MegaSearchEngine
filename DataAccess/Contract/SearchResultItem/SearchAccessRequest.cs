namespace DataAccess.Contract.SearchResultItem;

public record SearchAccessRequest(
    string PageSize,
    string PageIndex,
    string SearchText
    );