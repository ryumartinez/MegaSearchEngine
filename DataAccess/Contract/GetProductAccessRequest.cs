namespace DataAccess.Contract;

public record GetProductAccessRequest(
    int PageIndex,
    int PageSize,
    DateTime From,
    DateTime To
    );