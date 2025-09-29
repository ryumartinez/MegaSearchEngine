namespace DataAccess.Contract;

public record FilterAccessRequest(
    int PageIndex,
    int PageSize,
    DateTime From,
    DateTime To
    );