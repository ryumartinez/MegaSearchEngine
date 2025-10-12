namespace DataAccess.Contract;

public record GetProductAccessRequest(
    int PageIndex = 1,
    int PageSize = 100,
    DateTime? From = null,
    DateTime? To = null
    );