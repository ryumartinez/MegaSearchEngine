namespace DataAccess.Contract;

public record CreateProductAccessRequest(
    string Name,
    string Description,
    string Link,
    string SiteName);