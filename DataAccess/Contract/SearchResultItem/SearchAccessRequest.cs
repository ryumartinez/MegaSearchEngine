namespace DataAccess.Contract.SearchResultItem;

public record SearchAccessRequest(
    int PageSize,
    int PageIndex,
    string SearchText
    );